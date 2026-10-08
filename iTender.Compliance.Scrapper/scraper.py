from datetime import datetime
from typing import List, Optional

import httpx
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel


app = FastAPI(title="eTenders Scraper")


ETENDERS_BASE = "https://www.etenders.gov.za"
LIST_ENDPOINT = f"{ETENDERS_BASE}/Home/PaginatedTenderOpportunities"

PAGE_SIZE = 100
REQUEST_TIMEOUT = 60


class ScrapedTender(BaseModel):
    external_id: int
    tender_number: str
    title: str
    category: Optional[str] = None
    type: Optional[str] = None
    employer_name: Optional[str] = None
    status: Optional[str] = None
    advertised_date: Optional[str] = None
    closing_date: Optional[str] = None
    province: Optional[str] = None
    contact_person: Optional[str] = None
    contact_email: Optional[str] = None
    contact_telephone: Optional[str] = None
    conditions: Optional[str] = None
    briefing_session: bool = False
    briefing_compulsory: bool = False
    briefing_date: Optional[str] = None
    briefing_venue: Optional[str] = None
    e_submission: bool = False
    tender_url: Optional[str] = None


class ScrapeResponse(BaseModel):
    source: str
    scraped_on: str
    count: int
    tenders: List[ScrapedTender]


@app.get("/health")
async def health():
    return {"status": "ok"}


@app.post("/scrape/etenders", response_model=ScrapeResponse)
async def scrape_etenders(status: int = 1):
    """
    Fetch all tenders from the eTenders internal DataTables endpoint.
    status: 1 = Currently Advertised, 2 = Awarded, 3 = Closed, 4 = Cancelled
    """
    all_tenders: List[ScrapedTender] = []

    try:
        async with httpx.AsyncClient(
            timeout=REQUEST_TIMEOUT,
            headers={
                "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                "X-Requested-With": "XMLHttpRequest",
                "Accept": "application/json, text/javascript, */*; q=0.01",
            },
        ) as client:

            draw = 1
            start = 0
            total = None

            while True:
                params = {
                    "status": status,
                    "draw": draw,
                    "start": start,
                    "length": PAGE_SIZE,
                }

                r = await client.get(LIST_ENDPOINT, params=params)
                r.raise_for_status()

                payload = r.json()

                if total is None:
                    total = payload.get("recordsTotal", 0)

                rows = payload.get("data", [])

                if not rows:
                    break

                for row in rows:
                    all_tenders.append(_map_row(row, status))

                start += PAGE_SIZE
                draw += 1

                if total is not None and start >= total:
                    break

    except httpx.HTTPError as ex:
        raise HTTPException(
            status_code=502,
            detail=f"eTenders request failed: {ex}",
        )

    return ScrapeResponse(
        source=f"etenders-paginated-api?status={status}",
        scraped_on=datetime.utcnow().isoformat() + "Z",
        count=len(all_tenders),
        tenders=all_tenders,
    )


def _map_row(row: dict, status: int) -> ScrapedTender:
    tender_id = row.get("id")
    tender_no = (row.get("tender_No") or "").strip()

    return ScrapedTender(
        external_id=tender_id,
        tender_number=tender_no,
        title=(row.get("description") or "").strip(),
        category=row.get("category"),
        type=row.get("type"),
        employer_name=row.get("organ_of_State"),
        status=row.get("status"),
        advertised_date=row.get("date_Published"),
        closing_date=row.get("closing_Date"),
        province=row.get("province"),
        contact_person=row.get("contactPerson"),
        contact_email=row.get("email"),
        contact_telephone=row.get("telephone"),
        conditions=row.get("conditions"),
        briefing_session=bool(row.get("briefingSession")),
        briefing_compulsory=bool(row.get("briefingCompulsory")),
        briefing_date=row.get("compulsory_briefing_session"),
        briefing_venue=row.get("briefingVenue"),
        e_submission=bool(row.get("eSubmission")),
        tender_url=f"{ETENDERS_BASE}/Home/opportunities?id={status}#tender-{tender_id}",
    )
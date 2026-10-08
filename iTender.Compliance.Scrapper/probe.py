import asyncio
import httpx


async def main():
    async with httpx.AsyncClient(timeout=120) as client:
        r = await client.post("http://localhost:8000/scrape/etenders")
        r.raise_for_status()

        payload = r.json()

        print(f"Count: {payload['count']}")
        print()

        for t in payload["tenders"][:3]:
            print(f"- {t['tender_number']}")
            print(f"  {t['title'][:80]}")
            print(f"  {t['employer_name']} / {t['province']}")
            print(f"  Closing: {t['closing_date']}")
            print()


if __name__ == "__main__":
    asyncio.run(main())
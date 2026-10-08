# scrapers/tenderbulletins_scraper.py
from playwright.async_api import async_playwright
from bs4 import BeautifulSoup

async def scrape_tenderbulletins_playwright():
    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context(
            user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                       "AppleWebKit/537.36 (KHTML, like Gecko) "
                       "Chrome/120.0.0.0 Safari/537.36",
            locale="en-ZA",
        )
        page = await context.new_page()
        await page.goto(
            "https://tenderbulletins.co.za/tender-bulletin/",
            wait_until="networkidle",
            timeout=60_000,
        )
        # If Cloudflare challenge appears, wait it out
        await page.wait_for_timeout(5000)

        html = await page.content()
        await browser.close()

    soup = BeautifulSoup(html, "html.parser")
    # ... same selector logic ...
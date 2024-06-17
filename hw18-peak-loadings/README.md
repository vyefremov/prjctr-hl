# Homework: Peak Loadings

## Task

Describe solution that solves peak loadings problem for biggest european football website https://goal.com:
1. Analyze all types of pages on the site
2. Analyze and list possible sources of peak loadings
3. Describe possible solution for each type

## Solution

### Types of Pages

1. **Homepage** - the main page of the website that contains
   - live scores
   - the latest news
   - articles, and videos
2. **Scores Pages** - pages that contain scores, fixtures, and results
3. **News Pages** - pages that contain the latest news
4. **Tables Pages** - pages that contain the league tables
5. **Goal Studio Page** - the page that contains articles and content

### Sources of Peak Loadings

- **Major football events** (e.g. World Cup, UEFA Champions League, UEFA Europa League, Premier League): especially finals, big matches, and key knockout stages.
- **Breaking news**: transfer news, injuries, and scandals.
- **Match days**: pre-match build-up, post-match analysis, goal scored, and live updates.
- **Goal Studio Articles**: when there are popular articles, users want to read them.

### Solutions

- **Caching**:
  - full-page caching on demand (e.g. for the homepage and news pages)
  - cache-first approach for hot content (e.g. live scores shown on the homepage and other pages)
  - recent match results caching (shown on most pages)
  - recent news caching (e.g. latest news shown on the homepage and other pages)
  - recent articles caching (e.g. shown on the goal studio page)
  - cache **pre-warming** when upscaling to reduce the load on the databases
- **Scaling**
  - Before expected peak loadings (e.g. before the start of the match)
  - Based of historical data of peak loadings (predictive scaling)
- **Web sockets**: for live updates and live scores to reduce the number of requests
- **Content Delivery Network (CDN)**: to serve static content and reduce the load on the server especially for the homepage, news pages, and goal studio page

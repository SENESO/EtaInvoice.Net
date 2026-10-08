# Integration Testing

Live tests against the ETA e-invoicing **pre-production** environment live in
`test/EtaInvoice.IntegrationTests`. They are safe to run in CI without
credentials: every test calls `Assert.Ignore()` (skipped, not failed) when its
required environment variable is missing. Tests never run against production —
the fixture asserts the base URL is not the production endpoint.

## Credentials

| Variable | Required | Purpose |
|---|---|---|
| `ETA_CLIENT_ID` | Yes | Client id of your ERP system registered on the preprod portal |
| `ETA_CLIENT_SECRET` | Yes | Client secret of your ERP system |
| `ETA_BASE_URL` | No | Override the API base URL (default: `https://api.preprod.invoicing.eta.gov.eg`) |
| `ETA_TEST_SUBMISSION_UUID` | No | A submission uuid from your preprod taxpayer profile; used by the get-submission test |
| `ETA_ALLOW_SUBMIT` | No | Set to `true` to run the optional write test that submits a clearly-marked test invoice to preprod |

### Where to get preprod credentials

1. Go to the ETA portal at <https://www.eta.gov.eg> and open the **e-invoicing**
   section.
2. Complete the **preprod onboarding** (register on the pre-production taxpayer
   portal, not the production one).
3. In your preprod taxpayer profile, register your ERP system
   (representatives / add ERP) — the portal issues the **client id** and
   **client secret**.
4. Keep these credentials out of source control; pass them as environment
   variables or through your CI secrets store.

## Running

```bash
dotnet test test/EtaInvoice.IntegrationTests -c Release
```

With credentials:

```bash
ETA_CLIENT_ID=... ETA_CLIENT_SECRET=... dotnet test test/EtaInvoice.IntegrationTests -c Release
```

To also run the opt-in submission test against preprod:

```bash
ETA_CLIENT_ID=... ETA_CLIENT_SECRET=... ETA_ALLOW_SUBMIT=true dotnet test test/EtaInvoice.IntegrationTests -c Release
```

## What the tests cover

- `GetAccessToken_AgainstPreprod_ReturnsNonEmptyToken` — OAuth2 client-credentials
  token acquisition on `id.preprod.eta.gov.eg`.
- `SearchDocuments_AgainstPreprod_ReturnsResult` — read-only document search with
  a narrow 7-day submission-date window.
- `GetSubmission_AgainstPreprod_ReturnsInfo` — reads submission info for the uuid
  in `ETA_TEST_SUBMISSION_UUID` (skipped without it).
- `SubmitDocuments_AgainstPreprod_RequiresExplicitOptIn` — submits one unsigned
  invoice built from obviously test-only data (internal id prefixed
  `ETAINVOICE-INTEGRATION-TEST-`), validated by the SDK's
  `EtaDocumentValidator` first. Runs only when `ETA_ALLOW_SUBMIT=true`.

## Signing scope

Production e-invoices must carry a CAdES-BES signature made with the
taxpayer's USB eSeal token (or HSM). That key material can never be automated
in tests, so this SDK deliberately ships an injectable `IDocumentSigner`
instead of a built-in signer. The integration tests respect that scope: the
optional write test submits unsigned, which is acceptable on preprod only and
must never be used in production.

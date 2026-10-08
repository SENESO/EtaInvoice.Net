using System.Threading;
using System.Threading.Tasks;

namespace EtaInvoice.Signing
{
    /// <summary>
    /// Signs an e-invoice document for submission to the ETA.
    /// <para>
    /// Production e-invoices must carry a CAdES-BES signature produced with the
    /// taxpayer's eSeal certificate (usually held on a USB token or HSM). This
    /// SDK deliberately does <b>not</b> ship a built-in signer: the private key
    /// material must never leave your infrastructure. Implement this interface
    /// with your own signing service (USB token middleware, HSM, cloud key vault)
    /// and pass it to <c>EtaClient.SubmitDocumentsAsync</c>.
    /// </para>
    /// </summary>
    public interface IDocumentSigner
    {
        /// <summary>
        /// Signs the canonical JSON of a document.
        /// </summary>
        /// <param name="canonicalDocumentJson">
        /// The document serialized by <see cref="EtaCanonicalJson"/> (the document
        /// structure without the <c>signatures</c> section).
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// The signature value to embed in the document's <c>signatures</c> array:
        /// a Base64-encoded CAdES-BES structure, per the ETA digital signature
        /// format document.
        /// </returns>
        Task<string> SignAsync(string canonicalDocumentJson, CancellationToken cancellationToken = default);
    }
}

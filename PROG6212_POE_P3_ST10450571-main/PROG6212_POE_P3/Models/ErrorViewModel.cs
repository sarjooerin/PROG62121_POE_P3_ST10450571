namespace PROG6212_POE_P3.Models // FIX: Namespace changed from PROG6212_POE_P1.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
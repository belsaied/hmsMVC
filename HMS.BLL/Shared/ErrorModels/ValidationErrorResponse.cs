namespace HMS.BLL.Shared.ErrorModels
{
    public class ValidationErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<ValidationError> Errors { get; set; } = [];
    }
}

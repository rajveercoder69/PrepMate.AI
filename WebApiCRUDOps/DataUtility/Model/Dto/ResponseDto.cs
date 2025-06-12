namespace WebApiCRUDOps.DataUtility.Model.Dto
{
    public class ResponseDto
    {
        public object? Result { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string Error { get; set; }

    }
}

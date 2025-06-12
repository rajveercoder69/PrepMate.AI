namespace WebApi.Auth.MicroServer.Model.Dto
{
    public class PermissionResponseDto
    {
      
            public object? Result { get; set; }

            public bool IsSuccess { get; set; } = true;

            public string Error { get; set; }

        
    }
}

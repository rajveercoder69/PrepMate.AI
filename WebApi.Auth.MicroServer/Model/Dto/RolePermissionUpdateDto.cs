namespace WebApi.Auth.MicroServer.Model.Dto
{
    public class RolePermissionUpdateDto
    {
        public int RequestId {  get; set; }
        public string Status { get; set; }
        public DateTime UpdatedDateTime { get; set; }= DateTime.Now;
    }
}

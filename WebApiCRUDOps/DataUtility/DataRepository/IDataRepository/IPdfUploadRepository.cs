using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository.IDataRepository
{
    public interface IPdfUploadRepository:IDataRepository<PdfUpload>
    {
        void update(PdfUpload pdfUpload);
    }
}

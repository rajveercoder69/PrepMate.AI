namespace WebApiCRUDOps.DataUtility.DataRepository.IDataRepository
{
    public interface ISaveChange
    {
        IPersonProduct personproduct { get; }
        IPdfUploadRepository pdfUploadRepository{ get;}
        ILearningPdfsRepository learningPdfsRepository{ get;}
        void save();
    }
}

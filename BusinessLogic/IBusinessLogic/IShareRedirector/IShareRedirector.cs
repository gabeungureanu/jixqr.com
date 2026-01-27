using BusinessLogic.BLImplementation.ShareRedirector;
using DataTypes.ModelDataTypes;
using DataTypes.ModelDataTypes.Player;
using DataTypes.ModelDataTypes.ShareRedirector;
using DataTypes.ModelDataTypes.Subscription;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IBusinessLogic.IShareRedirector
{
    public interface IShareRedirector
    {
        //ShareRediretor InsertUploadFile(string AzurefileName, string fileExtension, string Duration, string FileSize, Guid UserID, int FileID);//Old one
        ShareRediretor InsertUploadFile(string AzurefileName, string fileExtension, string Duration, string FileSize, Guid UserID, int FileID,string UploadType, bool UploadTypeVal);
        Task<ShareRediretor> SaveFileDetails(Guid UserID, string FileID, string FileName, string ShareDescription, string RedirectURL, bool IsPublic, bool IsSpotlight, int DeliveryID, int DisplayOrder);
        ShareRediretor FetchFileExtension(string filename);
        Task<ShareRediretor> SaveFileStructure(Guid UserID, bool Isfolder, string foldername, int ID, string Folderlevel, string blobName);
        Task<ShareRediretor> DeleteFolder(int ID);
        ShareRediretor FetchContainerName(int FileID);
        //ShareRediretor CheckFileExist(int FileID);//Old one
        ShareRediretor CheckFileExist(int FileID,string UploadType, bool IsMusicAlbum);
        List<ShareRediretor> GetFilesDetails();
        ShareRediretor FilesDetailsGETByID(int FileID);
        string RemoveUploadedFile(int FileID);
        ShareRediretor RenameFolderName(int ID, string FolderName, bool IsFile);
        ShareRedirectorResponse GetShareRedirector_ContainerName(int ID);
        Task<ShareRediretor> DeleteFile(int FileID);
        ShareRediretor FetchRedirectURL(string filename);
        Task<ShareRediretor> FetchAzureURL(string filename);
        ShareRediretor FetchExternalID(int FileID);

        Task<List<ShareRediretor>> ActiveStructure(bool IsActive, int ID);
        List<SeriesModel> Fetch_Series(int authorID);
        List<ProductModel> Fetch_Product(int seriesId);
        List<DeliveryModel> Fetch_Delivery(int productId);

        Task<AlbumDetails> FetchAlbumDetails(Guid filename);
        //AlbumItems FetchAlbumSongs();
        AlbumItems FetchAlbumSongsBySearch(string text, string containerName);
        ShareMusicModel FetchShareMusicPopupByPopupType(string popupType);

        ShareRediretor InsertDuration(int FileID, string Duration);
        string InsertQRLogo(string FileName,string Logo ,int FileId, string BlobID);
        string RemoveQRLogo(int FileId, string BlobId);
        Product Insert_ProductThumbnail(int productID, string fileName, string filePath);
        Product Fetch_Productthumbnail(int productID);
        CustomQR Fetch_CustomQRDetails(int fileId, string BlobId);

        List<StoreModel> GetStoreDetails(int id);
       ShareRediretor MoveAuthorToStore(int storeID, int authorID);
       List<Blobfiles> FetchBlobFiles_byAuthor(int authorID);
        BlobConatiner FetchBlobConatiners(int storeID, int authorID);

        ShareRediretor Fetch_AlbumDetails(Guid shareId);
        StripePaymentDetails Fetch_PaymentDetails(int LibraryID, string Email);
        string UpdateShareOTP(int LibraryID, string Email, int OTP);
        string OTPVerify(int LibraryID, string Email, int OTP);
        string UpdateCookieDetails(int LibraryID, string Email, string Token);
        string VerifyLibraryPayment(int LibraryID, string Token);

        Task<List<ShareRediretor>> Get_Content_Record(int FileId);
        ShareRediretor InsertRedirectURL(int FileId, string URL);

        void updateIsSpotlight(bool IsSpotLight, string BlobId, int FileID);

        Task<ShareRediretor> GetBlobId_ByNanoId(string NanoId);
        int AddHitCount(string ItemId);
    }

}

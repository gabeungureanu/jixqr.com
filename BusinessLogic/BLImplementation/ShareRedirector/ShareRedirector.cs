using Anthropic.SDK.Messaging;
using BusinessLogic.IBusinessLogic.ICryptoService;
using BusinessLogic.IBusinessLogic.IShareRedirector;
using BusinessLogic.IBusinessLogic.IWebsiteSettingsService;
using Dapper;
using DataAccess;
using DataTypes.ModelDataTypes;
using DataTypes.ModelDataTypes.Common;
using DataTypes.ModelDataTypes.Home;
using DataTypes.ModelDataTypes.Player;
using DataTypes.ModelDataTypes.ShareRedirector;
using DataTypes.ModelDataTypes.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BLImplementation.ShareRedirector
{
    public class ShareRedirector : IShareRedirector
    {
        private readonly ICryptographyService _cryptographyService;
        private DbFactory _dBFactory;
        private IWebsiteSettings _websiteSettings;

        public ShareRedirector(ICryptographyService cryptographyService, DbFactory dbFactory, IWebsiteSettings websiteSettings)
        {
            _cryptographyService = cryptographyService;
            _websiteSettings = websiteSettings;
            _dBFactory = dbFactory;
        }


        public ShareRediretor InsertUploadFile(string AzurefileName, string fileExtension, string Duration, string FileSize, Guid UserID, int FileID, string UploadType, bool UploadTypeVal)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@Azurefilename", AzurefileName);
                dParam.Add("@fileExtension", fileExtension);
                dParam.Add("@FileID", FileID);
                dParam.Add("@Duration", Duration);
                dParam.Add("@FileSize", FileSize);
                dParam.Add("@UploadType", UploadType);
                dParam.Add("@UploadTypeVal", UploadTypeVal);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_InsertUploadedFile", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public async Task<ShareRediretor> SaveFileDetails(Guid UserID, string FileID, string FileName, string ShareDescription, string RedirectURL, bool IsPublic, bool IsSpotlight, int DeliveryID, int DisplayOrder)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@UserID", UserID);
                dParam.Add("@FileID", FileID);
                dParam.Add("@FileName", FileName);
                dParam.Add("@ShareDescription", ShareDescription);
                //dParam.Add("@RedirectURL", RedirectURL);
                //dParam.Add("@IsPublic", IsPublic);
                dParam.Add("@IsSpotlight", IsSpotlight);
                dParam.Add("@DeliveryID", DeliveryID);
                dParam.Add("@DisplayOrder", DisplayOrder);
                objshare = await _dBFactory.SelectCommand_SPAsync(objshare, "system_SaveFileDetail", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public ShareRediretor FetchFileExtension(string filename)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@fileName", filename);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_FetchFileExtension", dParam);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objshare;
        }

        public async Task<ShareRediretor> SaveFileStructure(Guid UserID, bool Isfolder, string foldername, int ID, string Folderlevel, string blobName)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                if (ID != 0 && Folderlevel != null)
                {
                    DynamicParameters dParam = new DynamicParameters();
                    dParam.Add("@UserID", UserID);
                    dParam.Add("@Isfolder", Isfolder);
                    dParam.Add("@foldername", foldername);
                    dParam.Add("@ID", ID);
                    dParam.Add("@Folderlevel", Folderlevel);
                    objshare = await _dBFactory.SelectCommand_SPAsync(objshare, "system_FileStructureSubFolder", dParam);
                }
                else
                {
                    DynamicParameters dParam = new DynamicParameters();
                    dParam.Add("@UserID", UserID);
                    dParam.Add("@Isfolder", Isfolder);
                    dParam.Add("@foldername", foldername);
                    dParam.Add("@blobName", blobName);
                    objshare = await _dBFactory.SelectCommand_SPAsync(objshare, "system_FileStructureFolder", dParam);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public async Task<ShareRediretor> DeleteFolder(int ID)
        {
            ShareRediretor objmodel = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ID);
                objmodel = await _dBFactory.SelectCommand_SPAsync(objmodel, "system_DeleteRecord", dParam);
            }

            catch (Exception)
            {
                throw;
            }
            return objmodel;
        }

        public ShareRediretor FetchContainerName(int FileID)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileID", FileID);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_ContainerName_Get", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        //public ShareRediretor CheckFileExist(int FileID)
        //{
        //    ShareRediretor objshare = new ShareRediretor();
        //    try
        //    {
        //        DynamicParameters dParam = new DynamicParameters();
        //        dParam.Add("@FileID", FileID);

        //        objshare = _dBFactory.SelectCommand_SP(objshare, "system_CheckFileExists", dParam);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return objshare;
        //}
        public ShareRediretor CheckFileExist(int FileID, string UploadType, bool UploadTypeVal)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileID", FileID);
                dParam.Add("@UploadType", UploadType);
                dParam.Add("@UploadTypeVal", UploadTypeVal);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_CheckFileExists", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public List<ShareRediretor> GetFilesDetails()
        {
            List<ShareRediretor> lstobj = new List<ShareRediretor>();
            try
            {
                lstobj = _dBFactory.SelectCommand_SP(lstobj, "system_Fetch_FileDetails");
            }
            catch (Exception)
            {
                throw;
            }
            return lstobj;
        }

        public ShareRediretor FilesDetailsGETByID(int FileID)
        {
            ShareRediretor objmodel = new ShareRediretor();
            List<AuthorModel> lstobj = new List<AuthorModel>();
            List<SeriesModel> lstobjseries = new List<SeriesModel>();
            List<ProductModel> lstobjproduct = new List<ProductModel>();
            List<DeliveryModel> lstobjdelivery = new List<DeliveryModel>();
            try
            {
                DynamicParameters objpara = new DynamicParameters();
                DynamicParameters objparaSeries = new DynamicParameters();
                DynamicParameters objparaProduct = new DynamicParameters();
                DynamicParameters objparaDelivery = new DynamicParameters();

                objpara.Add("FileID", FileID);
                objmodel = _dBFactory.SelectCommand_SP(objmodel, "system_FileDetails_Get_ByID", objpara);

                lstobj = _dBFactory.SelectCommand_SP(lstobj, "system_Getauthors", objpara);
                if (objmodel != null)
                {
                    objparaSeries.Add("@authorID", objmodel.AuthorID);
                    lstobjseries = _dBFactory.SelectCommand_SP(lstobjseries, "system_GetSeries", objparaSeries);
                }
                if (objmodel != null && lstobjseries.Count > 0)
                {
                    objparaProduct.Add("@seriesId", objmodel.SeriesID);
                    lstobjproduct = _dBFactory.SelectCommand_SP(lstobjproduct, "system_GetProduct", objparaProduct);
                }
                if (objmodel != null && lstobjproduct.Count > 0)
                {
                    objparaDelivery.Add("@productId", objmodel.ProductID);
                    lstobjdelivery = _dBFactory.SelectCommand_SP(lstobjdelivery, "system_GetDelivery", objparaDelivery);
                }
                //Bind authors
                if (lstobj.Count > 0)
                {
                    if (objmodel == null)
                    {
                        objmodel = new ShareRediretor();

                    }
                    objmodel.Authors = lstobj;
                }
                if (lstobjseries.Count > 0 && lstobjseries != null)
                {
                    objmodel.SeriesModel = lstobjseries;
                }
                if (lstobjproduct.Count > 0 && lstobjproduct != null)
                {
                    objmodel.ProductModel = lstobjproduct;
                }
                if (lstobjdelivery.Count > 0 && lstobjdelivery != null)
                {
                    objmodel.DeliveryModel = lstobjdelivery;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objmodel;
        }

        public string RemoveUploadedFile(int FileID)
        {
            string result = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileID", FileID);
                return _dBFactory.SelectCommand_SP<string>(result, "system_RemoveUploadedFile", dParam);
            }
            catch (Exception)
            {
                return "false";
            }
        }

        public ShareRediretor RenameFolderName(int ID, string FolderName, bool IsFile)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ID);
                dParam.Add("@FolderName", FolderName);
                dParam.Add("@IsFile", IsFile);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_RenameFolderName", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public ShareRedirectorResponse GetShareRedirector_ContainerName(int ID)
        {
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@ID", ID);

                var (Result1, Result2) = _dBFactory.QueryMultiple<ShareRediretor, BlobItem>(
                    "system_SharedRedirector_Container_Get",
                    dParam
                );

                return new ShareRedirectorResponse
                {
                    Redirector = Result1, // one record
                    Blobs = Result2.ToList()
                };
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<ShareRediretor> DeleteFile(int FileID)
        {
            ShareRediretor objmodel = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileId", FileID);
                objmodel = await _dBFactory.SelectCommand_SPAsync(objmodel, "system_Share_File_Delete", dParam);
            }

            catch (Exception)
            {
                throw;
            }
            return objmodel;
        }

        public ShareRediretor FetchRedirectURL(string filename)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@fileName", filename);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_FetchRedirectURL", dParam);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objshare;
        }

        public async Task<ShareRediretor> FetchAzureURL(string filename)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@fileName", filename);
                objshare = await _dBFactory.SelectCommand_SPAsync(objshare, "system_FetchAzureURL", dParam);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objshare;
        }

        /// <summary>
        /// Fetch GUID from redirect URL
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public ShareRediretor FetchExternalID(int FileID)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileID", FileID);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_ExternalID", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }



        public async Task<List<ShareRediretor>> ActiveStructure(bool IsActive, int ID)
        {
            try
            {
                List<ShareRediretor> LSTMODEL = new List<ShareRediretor>();
                var dParam = new DynamicParameters();
                dParam.Add("@ID", ID);
                dParam.Add("@IsActive", IsActive);

                LSTMODEL = await _dBFactory.SelectCommand_SP_List_Async(LSTMODEL, "system_ToggleHierarchyStatus", dParam);
                return LSTMODEL;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<SeriesModel> Fetch_Series(int authorID)
        {
            try
            {
                List<SeriesModel> LSTMODEL = new List<SeriesModel>();
                var dParam = new DynamicParameters();
                dParam.Add("@authorID", authorID);

                LSTMODEL = _dBFactory.SelectCommand_SP(LSTMODEL, "system_GetSeries", dParam);
                return LSTMODEL;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ProductModel> Fetch_Product(int seriesId)
        {
            try
            {
                List<ProductModel> LSTMODEL = new List<ProductModel>();
                var dParam = new DynamicParameters();
                dParam.Add("@seriesId", seriesId);

                LSTMODEL = _dBFactory.SelectCommand_SP(LSTMODEL, "system_GetProduct", dParam);
                return LSTMODEL;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<DeliveryModel> Fetch_Delivery(int productId)
        {
            try
            {
                List<DeliveryModel> LSTMODEL = new List<DeliveryModel>();
                var dParam = new DynamicParameters();
                dParam.Add("@productId", productId);
                LSTMODEL = _dBFactory.SelectCommand_SP(LSTMODEL, "system_GetDelivery", dParam);
                return LSTMODEL;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Product Insert_ProductThumbnail(int productID, string fileName, string filePath)
        {
            Product product = new Product();
            DynamicParameters objpara = new DynamicParameters();
            try
            {
                objpara.Add("ProductID", productID);
                objpara.Add("FileName", fileName);
                objpara.Add("FilePath", filePath);
                product = _dBFactory.SelectCommand_SP(product, "system_ProductThumbnail_Insert", objpara);
            }
            catch (Exception ex)
            {
                throw;
            }
            return product;
        }

        public Product Fetch_Productthumbnail(int productID)
        {
            Product Product = new Product();
            DynamicParameters objpara = new DynamicParameters();
            try
            {
                objpara.Add("ProductID", productID);
                Product = _dBFactory.SelectCommand_SP(Product, "system_Fetch_Productthumbnail", objpara);
            }
            catch (Exception ex)
            {
                throw;
            }
            return Product;
        }

        public CustomQR Fetch_CustomQRDetails(int fileId, string BlobId)
        {
            CustomQR customQR = new CustomQR();
            DynamicParameters objpara = new DynamicParameters();
            try
            {
                objpara.Add("FileId", fileId);
                objpara.Add("BlobId", BlobId);
                customQR = _dBFactory.SelectCommand_SP(customQR, "system_Fetch_CustomQR", objpara);
            }
            catch (Exception ex)
            {
                throw;
            }
            return customQR;
        }

        /// <summary>
        /// For the purpose to insert QR logo.
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Color"></param>
        /// <param name="FileId"></param>
        /// <returns></returns>
        public string InsertQRLogo(string FileName, string Color, int FileId, string BlobID)
        {
            DynamicParameters objpara = new DynamicParameters();
            ShowMessage showMessage = new ShowMessage();
            try
            {
                objpara.Add("Color", Color);
                objpara.Add("Logo", FileName);
                objpara.Add("FileId", FileId);
                objpara.Add("BlobID", BlobID);
                showMessage = _dBFactory.SelectCommand_SP(showMessage, "system_QRLogo_Insert", objpara);
            }
            catch (Exception)
            {
                throw;
            }
            return showMessage.Msg;
        }

        /// <summary>
        /// For the purpose to remove Logo from QR.
        /// </summary>
        /// <param name="FileId"></param>
        /// <returns></returns>
        public string RemoveQRLogo(int FileId, string BlobId)
        {
            DynamicParameters objpara = new DynamicParameters();
            ShowMessage showMessage = new ShowMessage();
            try
            {
                objpara.Add("FileId", FileId);
                objpara.Add("@BlobId", BlobId);
                showMessage = _dBFactory.SelectCommand_SP(showMessage, "system_QRLogo_Remove", objpara);
            }
            catch (Exception)
            {
                throw;
            }
            return showMessage.Msg;
        }
        #region Player
        public async Task<AlbumDetails> FetchAlbumDetails(Guid filename)
        {
            AlbumDetails objshare = new AlbumDetails();
            List<Album> objAlbum = new List<Album>();
            try
            {
                var objmodel = FetchFileExtension(filename.ToString());
                if (objmodel != null)
                {
                    objshare.FileName = objmodel.FileName;
                    objshare.FileExtension = objmodel.FileExtension;

                    DynamicParameters dParam = new DynamicParameters();
                    dParam.Add("@fileName", filename);
                    objAlbum = await _dBFactory.SelectCommand_SP_List_Async(objAlbum, "system_FetchAlbumDetails", dParam);
                    if (objAlbum != null)
                    {
                        objshare.Albums = objAlbum;
                        objshare.SongsCount = objAlbum.Count;
                        objshare.SumDuration = objAlbum[0].TotalDuration;

                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public AlbumItems FetchAlbumSongs()
        //{
        //    AlbumItems objAlbumItems = new AlbumItems();
        //    try
        //    {
        //        var objIsSpotLight = _dBFactory.SelectCommand(new List<IsSpotLight>(), "system_IsSpotlight_get");
        //        if (objIsSpotLight != null)
        //        {
        //            objAlbumItems.IsSpotLights = objIsSpotLight;

        //            var objProductDetails = _dBFactory.SelectCommand(new List<ProductDetails>(), "system_productName_get");
        //            if (objProductDetails != null)
        //            {
        //                foreach (var objProduct in objProductDetails)
        //                {
        //                    DynamicParameters parameters = new DynamicParameters();
        //                    parameters.Add("ProductID", objProduct.ProductID);

        //                    var productSongs = _dBFactory.SelectCommand_SP(new List<ProductsSong>(), "system_ProductsSong_get", parameters);
        //                    objProduct.ProductsSong = productSongs ?? new List<ProductsSong>();
        //                }

        //                // Filter out products that have no songs
        //                var filteredProducts = objProductDetails
        //                    .Where(p => p.ProductsSong != null && p.ProductsSong.Any())
        //                    .ToList();

        //                objAlbumItems.ProductDetails = filteredProducts;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    return objAlbumItems;
        //}


        public AlbumItems FetchAlbumSongsBySearch(string text, string containerName)
        {
            AlbumItems objAlbumItems = new AlbumItems();
            DynamicParameters objpara = new DynamicParameters();
            try
            {
                objpara.Add("SearchedText", text);
                objpara.Add("ContainerName", containerName);
                var objIsSpotLight = _dBFactory.SelectCommand_SP(new List<IsSpotLight>(), "system_IsSpotlight_get_BySearch", objpara);
                if (objIsSpotLight != null)
                {
                    objAlbumItems.IsSpotLights = objIsSpotLight;

                    var objProductDetails = _dBFactory.SelectCommand_SP(new List<ProductDetails>(), "system_productName_get_bySearch", objpara);
                    if (objProductDetails != null)
                    {
                        foreach (var objProduct in objProductDetails)
                        {
                            DynamicParameters parameters = new DynamicParameters();
                            parameters.Add("ProductID", objProduct.ProductID);
                            parameters.Add("SearchedText", null);

                            var productSongs = _dBFactory.SelectCommand_SP(new List<ProductsSong>(), "system_ProductsSong_get_Searched", parameters);
                            objProduct.ProductsSong = productSongs ?? new List<ProductsSong>();
                        }

                        // Filter out products that have no songs
                        var filteredProducts = objProductDetails
                            .Where(p => p.ProductsSong != null && p.ProductsSong.Any())
                            .ToList();

                        objAlbumItems.ProductDetails = filteredProducts;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return objAlbumItems;
        }


        /// <summary>
        /// Fetch GUID from redirect URL
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public ShareRediretor InsertDuration(int FileID, string Duration)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@FileID", FileID);
                dParam.Add("@Duration", Duration);
                objshare = _dBFactory.SelectCommand_SP(objshare, "system_InsertDuration", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public List<StoreModel> GetStoreDetails(int id)
        {
            DynamicParameters dParam = new DynamicParameters();
            List<StoreModel> lstStore = new List<StoreModel>();
            try
            {
                dParam.Add("id", id);
                lstStore = _dBFactory.SelectCommand_SP(lstStore, "system_StoreDetails_Get", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return lstStore;
        }

        public ShareRediretor MoveAuthorToStore(int storeID, int authorID)
        {
            ShareRediretor shareRediretor = new ShareRediretor();
            DynamicParameters dParam = new DynamicParameters();
            try
            {
                dParam.Add("StoreId", storeID);
                dParam.Add("AuthorID", authorID);
                shareRediretor = _dBFactory.SelectCommand_SP(shareRediretor, "system_MoveAuthor", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return shareRediretor;
        }

        public BlobConatiner FetchBlobConatiners(int storeID, int authorID)
        {
            BlobConatiner blobConatiner = new BlobConatiner();
            DynamicParameters dParam = new DynamicParameters();
            try
            {
                dParam.Add("StoreId", storeID);
                dParam.Add("AuthorID", authorID);
                blobConatiner = _dBFactory.SelectCommand_SP(blobConatiner, "system_FetchBlobConatiners", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return blobConatiner;
        }
        /// <summary>
        /// To get the blob files on author behalf.
        /// </summary>
        /// <param name="authorID"></param>
        /// <returns></returns>
        public List<Blobfiles> FetchBlobFiles_byAuthor(int authorID)
        {
            List<Blobfiles> lstBlob = new List<Blobfiles>();
            DynamicParameters dParam = new DynamicParameters();
            try
            {
                dParam.Add("AuthorID", authorID);
                lstBlob = _dBFactory.SelectCommand_SP(lstBlob, "system_FetchBlobFiles_byAuthor", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return lstBlob;
        }
        #endregion

        public ShareMusicModel FetchShareMusicPopupByPopupType(string popupType)
        {
            ShareMusicModel shareMusicModel = new ShareMusicModel();
            DynamicParameters Dparam = new DynamicParameters();
            try
            {
                Dparam.Add("PopupType", popupType);
                shareMusicModel = _dBFactory.SelectCommand_SP(shareMusicModel, "System_ShareMusicPopup_Fetch", Dparam);
            }
            catch (Exception)
            {
                throw;
            }
            return shareMusicModel;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareId"></param>
        /// <returns></returns>
        public ShareRediretor Fetch_AlbumDetails(Guid shareId)
        {
            ShareRediretor shareRediretor = new ShareRediretor();
            DynamicParameters dParam = new DynamicParameters();
            try
            {
                dParam.Add("AzurefileID", shareId);
                shareRediretor = _dBFactory.SelectCommand_SP(shareRediretor, "System_fetch_AlbumDetails", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return shareRediretor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareId"></param>
        /// <returns></returns>
        public StripePaymentDetails Fetch_PaymentDetails(int LibraryID, string Email)
        {
            StripePaymentDetails objStripePaymentDetails = new StripePaymentDetails();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("LibraryID", LibraryID);
                dParam.Add("Email", Email);
                objStripePaymentDetails = _dBFactory.SelectCommand_SP(objStripePaymentDetails, "System_fetch_PaymentDetails", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objStripePaymentDetails;
        }

        /// <summary>
        /// UPdate OTP
        /// </summary>
        /// <param name="LibraryID"></param>
        /// <param name="Email"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        public string UpdateShareOTP(int LibraryID, string Email, int OTP)
        {
            string retval = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("LibraryID", LibraryID);
                dParam.Add("Email", Email);
                dParam.Add("OTP", OTP);
                retval = _dBFactory.SelectCommand_SP(retval, "System_EmailOTP_update", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return retval;
        }
        /// <summary>
        /// UPdate OTP
        /// </summary>
        /// <param name="LibraryID"></param>
        /// <param name="Email"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        public string UpdateCookieDetails(int LibraryID, string Email, string Token)
        {
            string retval = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("LibraryID", LibraryID);
                dParam.Add("Email", Email);
                dParam.Add("Token", Token);
                retval = _dBFactory.SelectCommand_SP(retval, "System_CookieToken_update", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return retval;
        }
        /// <summary>
        /// OTP verify
        /// </summary>
        /// <param name="LibraryID"></param>
        /// <param name="Email"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        public string OTPVerify(int LibraryID, string Email, int OTP)
        {
            string retval = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("LibraryID", LibraryID);
                dParam.Add("Email", Email);
                dParam.Add("OTP", OTP);
                retval = _dBFactory.SelectCommand_SP(retval, "System_EmailOTP_Verify", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return retval;
        }

        /// <summary>
        /// OTP verify
        /// </summary>
        /// <param name="LibraryID"></param>
        /// <param name="Email"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        public string VerifyLibraryPayment(int LibraryID, string Token)
        {
            string retval = string.Empty;
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("LibraryID", LibraryID);
                dParam.Add("Token", Token);
                retval = _dBFactory.SelectCommand_SP(retval, "System_Payment_Verify", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return retval;
        }
        //============================================
        //PURPOSE: TO GET THE UPLOADED CONTENT RECORD
        //CREATED BY: M.F
        //CREATED DATE: 26-AUG-2025
        //============================================
        public async Task<List<ShareRediretor>> Get_Content_Record(int FileId)
        {
            List<ShareRediretor> lstobjmodel = new List<ShareRediretor>();
            DynamicParameters Dparam = new DynamicParameters();
            try
            {
                Dparam.Add("FileId", FileId);
                lstobjmodel = await _dBFactory.SelectCommand_SP_List_Async(lstobjmodel, "Get_Content_Record", Dparam);
            }
            catch (Exception)
            {
                throw;
            }
            return lstobjmodel;
        }


        public ShareRediretor InsertRedirectURL(int FileId, string URL)
        {
            ShareRediretor objmodel = new ShareRediretor();
            DynamicParameters Dparam = new DynamicParameters();
            try
            {
                Dparam.Add("FileId", FileId);
                Dparam.Add("URL", URL);
                objmodel = _dBFactory.SelectCommand_SP(objmodel, "System_InsertRedirectURL", Dparam);
            }
            catch (Exception)
            {
                throw;
            }
            return objmodel;
        }


        //======================================
        //PURPOSE: TO UPDATE THE IS SPOTLIGHT.
        //CREATEDBY: M.F
        //CREATEDDATE: 28-08-2025
        //=======================================
        public void updateIsSpotlight(bool IsSpotLight, string BlobId, int FileID)
        {
            DynamicParameters Dparam = new DynamicParameters();
            try
            {
                Dparam.Add("IsSpotLight", IsSpotLight);
                Dparam.Add("BlobId", BlobId);
                Dparam.Add("FileID", FileID);
                var result = _dBFactory.InsertCommand_SPExecute("System_UpdateIsSpotlight", Dparam);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ShareRediretor> GetBlobId_ByNanoId(string NanoId)
        {
            ShareRediretor objshare = new ShareRediretor();
            try
            {
                DynamicParameters dParam = new DynamicParameters();
                dParam.Add("@NanoId", NanoId);
                objshare = await _dBFactory.SelectCommand_SPAsync(objshare, "System_getBlobId_BynanoId", dParam);
            }
            catch (Exception)
            {
                throw;
            }
            return objshare;
        }

        public int AddHitCount(string ItemId)
        {
            DynamicParameters Dparam = new DynamicParameters();
            int retval = 0;
            try
            {
                Dparam.Add("ItemId", ItemId);
                retval = _dBFactory.SelectCommand_SP(retval, "system_RedirectHits_Insert", Dparam);
            }
            catch (Exception)
            {
                throw;
            }
            return retval;
        }
    }
}

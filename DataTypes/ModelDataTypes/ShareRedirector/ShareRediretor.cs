using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ShareRedirector;

public class ShareRediretor
{
    public int FileID { get; set; }
    public string? NanoId { get; set; }
    public string? FileName { get; set; }
    public string? ShareDescription { get; set; }
    public string? Azurefilename { get; set; }
    public string FreeAzureURl { get; set; }
    public string? FileExtension { get; set; }
    public string? RedirectURL { get; set; }
    public string? RedirectLink { get; set; }
    public string? RedirectURLQRCode { get; set; }
    public string? URLMapLink { get; set; }
    public string? Product { get; set; }
    public string? ProductImage { get; set; }
    public int DeliveryID { get; set; }
    public string? Delivery { get; set; }
    public int ProductID { get; set; }
    public string? Series { get; set; }
    public int SeriesID { get; set; }
    public string? Author { get; set; }
    public int AuthorID { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public bool IsSpotlight { get; set; }
    public bool Status { get; set; }
    public bool IsFileUpdate { get; set; }
    public string? Message { get; set; }
    public string? MarketingQRName { get; set; }
    public string? PaymentQRName { get; set; }
    public bool IsContentAvailable { get; set; }
    //----------------------------------
    public int RootId { get; set; }
    public string? FolderName { get; set; }
    public string? ContainerName { get; set; }
    public bool IsFolder { get; set; }
    public bool IsFile { get; set; }
    public int ID { get; set; }
    public int CurrentFileID { get; set; }
    public string? Folderlevel { get; set; }
    public string? Duration { get; set; }
    public int DisplayOrder { get; set; }
    public string? QRColor { get; set; }
    public string? QRLogo { get; set; }
    public bool IsMusicAlbum { get; set; } = false;
    public bool IsAudioBook { get; set; } = false;
    public bool IsVideoFile { get; set; } = false;
    public bool IsPDFFiles { get; set; } = false;

    public List<AuthorModel> Authors { get; set; } = new List<AuthorModel>();
    public List<SeriesModel> SeriesModel { get; set; } = new List<SeriesModel>();
    public List<ProductModel> ProductModel { get; set; } = new List<ProductModel>();
    public List<DeliveryModel> DeliveryModel { get; set; } = new List<DeliveryModel>();

}

public class AuthorModel
{
    public int AuthorID { get; set; }
    public string? AuthorName { get; set; } = string.Empty;
}

public class SeriesModel
{
    public int SeriesID { get; set; }
    public string? SeriesName { get; set; } = string.Empty;
}

public class ProductModel
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; } = string.Empty;
}

public class DeliveryModel
{
    public int DeliveryID { get; set; }
    public string? DeliveryName { get; set; } = string.Empty;
}

public class StoreModel
{
    public int StoreID { get; set; }
    public int SelectedStoreID { get; set; }
    public string? StoreName { get; set; } = string.Empty;
}

public class Blobfiles
{
    public int FileId { get; set; }
    public string BlobFileName { get; set; }
    public string FileExtension { get; set; }
}

public class BlobConatiner
{
    public string SourceContainer { get; set; }
    public string TargetContainer { get; set; }
}

public class MoveBlobResult
{
    public bool IsSuccess { get; set; }
    public List<string> SuccessFiles { get; set; } = new();
    public List<string> FailedFiles { get; set; } = new();
}

public class BlobItem
{
    public string AzureFileName { get; set; }
    public string FileExtension { get; set; }
}
public class ShareRedirectorResponse
{
    public ShareRediretor Redirector { get; set; }
    public List<BlobItem> Blobs { get; set; }
}

using Library.Models.Catalog;
using Library.Models.CheckoutModels;
using LibraryData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ILibraryAsset _assets;
        private readonly ICheckout _checkout;

        public CatalogController(ILibraryAsset assets, ICheckout checkout)
        {
            _assets = assets;
            _checkout = checkout;
        }


        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();

            var listingResult = assetModels
                .Select(result => new AssetndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _assets.GetType(result.Id)
                });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var asset = _assets.GetById(id);

            var currentHolds = _checkout.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced = _checkout.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronName = _checkout.GetCurrentHoldPatronName(a.Id)
                });


            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetIsbn(id),
                CheckoutHistory = _checkout.GetCheckoutHistory(id),
                LatestCheckout = _checkout.GetLatestCheckout(id),
                PatronName = _checkout.GetCurrentCheckoutPatron(id),
                CurrentHolds = currentHolds

            };

            return View(model);

        }

        public IActionResult Checkout(int id)
        {
            var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkout.IsCheckedOut(id)
            };

            return View(model);
        }

        public IActionResult CheckIn(int id)
        {
            _checkout.CheckInItem(id);
            return RedirectToAction("Detail", new { id });
        }

        public IActionResult MarkLost(int assetId)
        {
            _checkout.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId});
        }
        public IActionResult MarkFound(int assetId)
        {
            _checkout.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }
        public IActionResult Hold(int id)
        {
            var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkout.IsCheckedOut(id),
                HoldCount = _checkout.GetCurrentHolds(id).Count()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceCheckout(int assetId)
        {
            _checkout.CheckInItem(assetId);

            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkout.PlaceHold(assetId, libraryCardId);

            return RedirectToAction("Detail", new { id = assetId });
        }
    }
}


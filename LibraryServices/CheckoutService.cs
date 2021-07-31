using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private readonly LibraryContext _context;

        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public void CheckInItem(int assetId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);


            var checkout = _context.Checkouts.FirstOrDefault(co => co.LibraryAssets.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }

            var history = _context.CheckoutHistories
                .FirstOrDefault(h => h.LibraryAssets.Id == assetId && h.CheckedIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }

            var currentHolds = _context.Holds
                .Include(h => h.LibraryAssets)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAssets.Id == assetId);

            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
            }

            UpdateAssetsStatus(assetId, "Available");
            _context.SaveChanges();

        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds.OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckOutItem(assetId, card.Id);

        }

        private void UpdateAssetsStatus(int assetId, string v)
        {
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name == v);
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
            }

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            UpdateAssetsStatus(assetId, "Checked Out");
            var libraryCard = _context.LibraryCards.Include(card => card.Checkouts).FirstOrDefault(card => card.Id == libraryCardId);
            var checkout = new Checkout
            {
                LibraryAssets = item,
                LibraryCard = libraryCard,
                Since = DateTime.Now,
                Until = GetDefaultCheckoutTime(DateTime.Now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = DateTime.Now,
                LibraryAssets = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);

            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        public bool IsCheckedOut(int id)
        {
            var isCheckedOut = _context.Checkouts.Where(co => co.LibraryAssets.Id == id).Any();

            return isCheckedOut;
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(checkout => checkout.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(h => h.LibraryAssets)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAssets.Id == id);
        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAssets)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;

            var patron = _context.Patrons.Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstaName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
           return _context.Holds
                .Include(h => h.LibraryAssets)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId).HoldPlaced;
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAssets)
                .Where(h => h.LibraryAssets.Id == id);
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts.Where(c => c.LibraryAssets.Id == assetId).OrderByDescending(c => c.Since).FirstOrDefault();
        }

        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name == "Available");

            var checkout = _context.Checkouts.FirstOrDefault(co => co.LibraryAssets.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }

            var history = _context.CheckoutHistories
                .FirstOrDefault(h => h.LibraryAssets.Id == assetId && h.CheckedIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }
            _context.SaveChanges();
        }

        public void MarkLost(int assetId)
        {
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name == "Lost");

            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            var card = _context.LibraryCards
                .FirstOrDefault(c => c.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetsStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAssets = asset,
                LibraryCard = card
            };

            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);

            if (checkout == null)
            {
                return "";
            }

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons.Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstaName + " " + patron.LastName; 
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(co => co.LibraryAssets)
                .Include(co => co.LibraryCard)
                .FirstOrDefault(co => co.LibraryAssets.Id == assetId);
        }

   
    }
}

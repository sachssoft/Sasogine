//using Sachssoft.Sasogine.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Sachssoft.Sasogine.Platform
//{
//    public class DesktopPurchaseService : IInAppPurchaseService
//    {
//        public bool CanMakePayments => throw new NotImplementedException();

//        public event Action<string>? PurchaseStarted;
//        public event Action<string, PurchaseResult>? PurchaseFinished;

//        public Task<IReadOnlyList<IProductInfo>> GetAvailableProductsAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsPurchased(string productId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<PurchaseResult> PurchaseAsync(string productId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task RestorePurchasesAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

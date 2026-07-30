using Nop.Core.Domain.Discounts;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;

public static class ObjectModelFactory
{
    public static Discount CreateDiscount(decimal discountAmount, DateTime endDateUtc, string name, string adminComment = "")
    {
        return new()
        {
            AdminComment = adminComment,
            AppliedToSubCategories = false,
            DiscountAmount = discountAmount,
            DiscountLimitation = DiscountLimitationType.Unlimited,
            DiscountType = DiscountType.AssignedToSkus,
            IsCumulative = false,
            RequiresCouponCode = true,
            Name = name,
            IsActive = true,
            EndDateUtc = endDateUtc,
            StartDateUtc = DateTime.UtcNow.ToUniversalTime(),
            CouponCode = Guid.NewGuid().ToString()
        };
    }
}

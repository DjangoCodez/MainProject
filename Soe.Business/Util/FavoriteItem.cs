using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    public class FavoriteItem
    {
        public int FavoriteId { get; set; }
        public string FavoriteName { get; set; }
        public string FavoriteUrl { get; set; }
        public int? FavoriteCompany { get; set; }
        public bool IsDefault { get; set; }

        //Only for default options
        public SoeFavoriteOption FavoriteOption { get; set; }
    }

    public class MenuFavoriteItem
    {
        public int FavoriteId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsSupportFavorite { get; set; }
    }
}

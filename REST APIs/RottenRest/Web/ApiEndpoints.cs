namespace RottenRest.Web;

public static class ApiEndpoints
{
    public const string ApiBase = "api";

    public static class Movies
    {
        public const string Prefix = $"{ApiBase}/movies";

        public const string Create = "";
        public const string Get = "{idOrSlug}";
        public const string GetAll = "";
        public const string Update = "{id:guid}";
        public const string Delete = "{id:guid}";

        public const string Rate = "{id:guid}/ratings";
        public const string DeleteRating = "{id:guid}/ratings";
    }

    public static class Ratings
    {
        public const string Prefix = $"{ApiBase}/ratings";

        public const string GetUserRatings = "me";
    }
}

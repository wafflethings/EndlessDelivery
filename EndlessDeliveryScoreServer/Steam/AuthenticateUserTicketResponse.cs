namespace EndlessDeliveryScoreServer
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Params
    {
        public string Result { get; set; }
        public string SteamId { get; set; }
        public string OwnerSteamId { get; set; }
        public bool VacBanned { get; set; }
        public bool PublisherBanned { get; set; }
    }

    public class Response
    {
        public Params @params { get; set; }
    }

    public class AuthenticateUserTicketResponse
    {
        public Response response { get; set; }
    }
}
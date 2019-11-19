using Newtonsoft.Json.Linq;

namespace Common
{
    public class ABUnit
    {
        public string ABName;
        public string ABPath;

        public void Parse( JObject jObj )
        {
            if ( jObj == null )
            {
                return;
            }

            JToken token = null;
            
            if ( jObj.TryGetValue( "ABName", out token) )
            {
                ABName = token.ToString();
            }
            
            if ( jObj.TryGetValue( "ABPath", out token) )
            {
                ABPath = token.ToString();
            }
        }
    }
}
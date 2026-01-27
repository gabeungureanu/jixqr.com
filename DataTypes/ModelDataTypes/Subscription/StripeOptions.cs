using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.Subscription
{
    public class StripeOptions
    {
        public string PublishableKey { get; set; } = null!; //= "pk_test_51O6vGhBJlSkCt4jcbuI21N35p8HIERttzrnqSEZVOkezAY1xBrtxiP7dKPzSm0KJhMXPoRiJPULZvapRm42IeBWY00xPD48OuY";
        public string SecretKey { get; set; } = null!;//= "sk_test_51O6vGhBJlSkCt4jc72EkyTVHLHAjXihQPEUR4k1vC8Hd1xJJZBGA5M4Zx5btBNxnnpRuOGZDN8YIuUIsLsIMDb5J00Baq6PfJ9";
        public string WebhookSecret { get; set; } = null!;//= "sk_test_51O6vGhBJlSkCt4jc72EkyTVHLHAjXihQPEUR4k1vC8Hd1xJJZBGA5M4Zx5btBNxnnpRuOGZDN8YIuUIsLsIMDb5J00Baq6PfJ9";

        public string BasicPrice { get; set; } = null!;//= "995";
        public string ProPrice { get; set; } = null!;//= "995";
        public string Domain { get; set; } = null!;//= "https://localhost:7187";

    }
}

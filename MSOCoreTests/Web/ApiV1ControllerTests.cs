using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MSOCoreTests.Web
{
    [TestClass]
    public class ApiV1ControllerTests
    {
        [TestMethod]
        public void AddEventEntry_Works()
        {
            var json = "{ data: 'bar' }";
            var url = "http://**REDACTEDAwsDbName**.juliahayward.com/apiv1/AddEventEntry";

            using (WebClient client = new WebClient())
            {
                byte[] response = client.UploadValues(url, new NameValueCollection()
                    {
                        { "data", json }
                    });

                string result = System.Text.Encoding.UTF8.GetString(response);
                Assert.IsTrue(result.Contains("{\"success\":true}"));
            }
        }
    }
}

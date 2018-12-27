using System;
using System.Collections.Generic;
using System.Text;
using NEO_Block_API.lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Block_API.Controllers
{
    class Business
    {
        mongoHelper mh = new mongoHelper();

        public JObject getNgdBlockcount(string neoRpcUrl_mainnet)
        {
            httpHelper hh = new httpHelper();
            var resp = hh.Post(neoRpcUrl_mainnet, "{'jsonrpc':'2.0','method':'getblockcount','params':[],'id':1}", System.Text.Encoding.UTF8, 1);

            string ret = (string)JObject.Parse(resp)["result"];
            JObject ob = new JObject();
            ob.Add("blockcount", ret);
            return ob;
        }

        public JObject getOperatedFee(string mongodbConnStr, string mongodbDatabase, string addr)
        {
            string findFliter = string.Empty;
            if (addr.Length>0)
            {
                findFliter = "{addr:'" + addr + "'}";
            }
            else
            {
                findFliter = "{}";
            }
            JArray arry = mh.GetData(mongodbConnStr, mongodbDatabase, "operatedFee", findFliter);
           
            decimal sds_value = 0; //所有sdsFee总值
            decimal sdusd_value = 0; //所有sdusdFee总值
            foreach (JObject operated in arry)
            {
                sds_value += (decimal)operated["sdsFee"];
                sdusd_value += (decimal)operated["sdusdFee"];
            }
            JObject ob = new JObject();
            ob.Add("sdsFee", sds_value);
            ob.Add("sdusdFee", sdusd_value);
            return ob;
        }
    }
}

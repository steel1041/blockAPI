using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Numerics;

namespace Block_API.Controllers
{
    public class NEP552
    {
       
        //public class OperatorFee
        //{
        //    public OperatorFee(int Blockindex, string Txid, int N, JObject notification, int decimals, DateTime time)
        //    {

        //    }

        //    public OperatorFee(JObject tfJ) {
        //        asset = (string)tfJ["asset"];
        //        blockindex = (int)tfJ["blockindex"];
        //        addr = (string)tfJ["addr"];
        //        sarTxid = (string)tfJ["sarTxid"];
        //        txid = (string)tfJ["txid"];
        //        n = (int)tfJ["n"];
        //        sdusdFee = (decimal)tfJ["sdusdFee"];
        //        sdsFee = (decimal)tfJ["sdsFee"];
        //        blocktime = (DateTime)tfJ["blocktime"];
        //    }
        //    public ObjectId _id { get; set; }
        //    public string asset { get; set; }
        //    public int blockindex { get; set; }
        //    public string addr { get; set; }
        //    public string sarTxid { get; set; }
        //    public string txid { get; set; }
        //    public int n { get; set; }
        //    public decimal sdusdFee { get; set; }
        //    public decimal sdsFee { get; set; }
        //    public DateTime blocktime { get; set; }
        //}
    }
}

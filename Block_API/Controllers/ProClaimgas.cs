using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Block_API.Controllers
{
    [BsonIgnoreExtraElements]
    class ProClaimgas
    {
        public ProClaimgas(string ClaimAddr, string Txid, int N, string UsedTxid,int Value,decimal Gas,int Status,DateTime Create,DateTime Pro)
        {
            claimAddr = ClaimAddr;
            txid = Txid;
            n = N;
            usedTxid = UsedTxid;
            value = Value;
            gas = Gas;
            status = 1;  //1:未处理，2:已处理
            create = Create; //创建时间
            pro = Pro;  //处理时间

        }

        public ObjectId _id { get; set; }
        public string claimAddr { get; set; }
        public int blockindex { get; set; }
        public string txid { get; set; }
        public string usedTxid { get; set; }
        public int n { get; set; }
        public int value { get; set; }
        public decimal gas { get; set; }
        public int status { get; set; }
        public DateTime create { get; set; }
        public DateTime pro { get; set; }
    }
}

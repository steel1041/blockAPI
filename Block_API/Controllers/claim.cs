using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NEO_Block_API.lib;
using System.IO;
using Block_API.Controllers;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading;

namespace NEO_Block_API.Controllers
{
    public class Claim
    {
        mongoHelper mh = new mongoHelper();

        Contract ct = new Contract();

        Transaction tx = new Transaction();

        public JObject getClaimGas(string mongodbConnStr, string mongodbDatabase, string address, bool isGetUsed = true)
        {
            decimal issueGas = 0;

            string findFliter = string.Empty;
            if (isGetUsed)
            {
                //已使用，未领取(可领取GAS)
                findFliter = "{addr:'" + address + "','asset':'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b','used':{$ne:''},'claimed':''}";
            }
            else
            {
                //未使用，未领取(不可领取GAS)
                findFliter = "{addr:'" + address + "','asset':'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b','used':'','claimed':''}";
            }

            //统计有多少NEO UTXO
            long utxoCount = mh.GetDataCount(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

            JObject J = new JObject();

            //只有UTXO小于等于50才处理
            int UTXOThreshold = 50;
            if (utxoCount <= UTXOThreshold)
            {
                JArray gasIssueJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);


                foreach (JObject utxo in gasIssueJA)
                {
                    int start = (int)utxo["createHeight"];
                    int end = -1;
                    if (isGetUsed)
                    {
                        end = (int)utxo["useHeight"] - 1; //转出的这块的gas属于转入地址
                    }
                    else
                    {
                        //未花费以目前高度计算
                        end = (int)mh.Getdatablockheight(mongodbConnStr, mongodbDatabase).First()["blockDataHeight"];
                    }
                    int value = (int)utxo["value"];

                    decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start - 1);
                    decimal issueGasInBlock = countGas(start, end);

                    issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;
                }

                J.Add("gas", issueGas);
                J.Add("claims", gasIssueJA);
                J.Add("count", utxoCount);
            }
            else
            {
                //大于50条记录就取50条记录进行操作
                JArray gasIssueJA = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "utxo","{}",50,1,findFliter);

                foreach (JObject utxo in gasIssueJA)
                {
                    int start = (int)utxo["createHeight"];
                    int end = -1;
                    if (isGetUsed)
                    {
                        end = (int)utxo["useHeight"] - 1; //转出的这块的gas属于转入地址
                    }
                    else
                    {
                        //未花费以目前高度计算
                        end = (int)mh.Getdatablockheight(mongodbConnStr, mongodbDatabase).First()["blockDataHeight"];
                    }
                    int value = (int)utxo["value"];

                    decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start - 1);
                    decimal issueGasInBlock = countGas(start, end);

                    issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;
                }

                J.Add("gas", issueGas);
                J.Add("claims", gasIssueJA);
                J.Add("count", utxoCount);
            }

            return J;
        }

        public JObject getClaimGasByTx(string mongodbConnStr, string mongodbDatabase, string txid,int n)
        {
            decimal issueGas = 0;

            string findFliter = "{txid:'" + txid +"',n:"+ n + ",'asset':'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b'}";

            JObject J = new JObject();

            JArray gasIssueJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

            JArray claims = new JArray();
            foreach (JObject utxo in gasIssueJA)
            {
                //JObject utxo = (JObject)gasIssueJA[0];
                JObject gas = new JObject();
                string used = (string)utxo["used"];
                int start = (int)utxo["createHeight"];
                int end = -1;
                if (used.Length > 0)
                {
                    end = (int)utxo["useHeight"] - 1; //转出的这块的gas属于转入地址
                }
                else
                {
                    //未花费以目前高度计算
                    end = (int)mh.Getdatablockheight(mongodbConnStr, mongodbDatabase).First()["blockDataHeight"];
                }
                int value = (int)utxo["value"];

                //转入的这个块，属于当前地址，由于差额计算方法，需要开始-1
                decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start - 1);

                decimal issueGasInBlock = countGas(start, end);

                issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;

                gas.Add("addr",(string)utxo["addr"]);
                gas.Add("txid",txid);
                gas.Add("n", (int)utxo["n"]);
                gas.Add("gas", issueGas);

                claims.Add(gas);
            }

            //J.Add("gas", issueGas);
            J.Add("claims", claims);
            return J;
        }

        //按1亿NEO计算
        private decimal countGas(int start, int end)
        {
            int step = 200 * 10000;
            int gasInBlock = 8;
            decimal gasCount = 0;
            for (int i = 0; i < 22; i++)
            {
                int forStart = i * step;
                int forEnd = (i + 1) * step - 1;

                //如果utxo使用高度小于循环开始高度，表示当前utxo都不在后续区间中
                if (end < forStart) { break; }

                //在区间内才计算
                if (start <= forEnd)
                {
                    if (start > forStart) { forStart = start; }
                    if (end < forEnd) { forEnd = end; }

                    gasCount += (forEnd - forStart + 1) * gasInBlock;
                }

                //每200万块，衰减一个gas，如果衰减到1则不变
                if (gasInBlock > 1) { gasInBlock--; }
            }

            return gasCount;
        }

        public JObject claimContract2(string mongodbConnStr, string mongodbDatabase, string conAddr, string addrClaim, string url, string hash)
        {
            JObject claimGas = getClaimGas(mongodbConnStr, mongodbDatabase, conAddr, true);

            var assetIDStr = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7"; //选择GAS支付合约调用费用
            var assetID = assetIDStr.Replace("0x", "").HexString2Bytes().Reverse().ToArray();

            //构建交易体
            ThinNeo.Transaction claimTran = new ThinNeo.Transaction
            {
                type = ThinNeo.TransactionType.ClaimTransaction,//领取Gas合约
                attributes = new ThinNeo.Attribute[0],
                inputs = new ThinNeo.TransactionInput[0],
                outputs = new ThinNeo.TransactionOutput[1],
                extdata = new ThinNeo.ClaimTransData()
            };

            claimTran.outputs[0] = new ThinNeo.TransactionOutput
            {
                assetId = assetID,
                toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(addrClaim),
                value = (decimal)claimGas["gas"]
            };

            List<ThinNeo.TransactionInput> claimVins = new List<ThinNeo.TransactionInput>();
            foreach (JObject j in (JArray)claimGas["claims"])
            {
                claimVins.Add(new ThinNeo.TransactionInput
                {
                    hash = ThinNeo.Debug.DebugTool.HexString2Bytes(((string)j["txid"]).Replace("0x", "")).Reverse().ToArray(),
                    index = (ushort)j["n"]
                });
            }

            (claimTran.extdata as ThinNeo.ClaimTransData).claims = claimVins.ToArray();

            JObject contract = ct.getContractState(url, hash);
            //做智能合约的签名
            byte[] iscript = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                sb.EmitPushString("whatever");
                sb.EmitPushNumber(250);
                iscript = sb.ToArray();
            }
            byte[] conbytes = ThinNeo.Debug.DebugTool.HexString2Bytes((string)contract["script"]);
            claimTran.AddWitnessScript(conbytes, iscript);

            var trandata = claimTran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            JObject result = tx.sendrawtransaction(url, strtrandata);
            bool re = (bool)result["sendrawtransactionresult"];
            Console.WriteLine("得到的结果是：" + re);
            if (re)
            {
                Console.WriteLine("txid：" + (string)result["txid"]);
            }
            else {
                Console.WriteLine("errorMessage：" + (string)result["errorMessage"]);

            }
            return result;
        }

        public JObject claimContract(string mongodbConnStr, string mongodbDatabase, string conAddr, string addrClaim, string url, string hash)
        {
            JObject claimGas = getClaimGas(mongodbConnStr, mongodbDatabase, conAddr, true);

            string unsignHexTx = tx.getClaimTxHex(addrClaim,claimGas);

            byte[] txScript = unsignHexTx.HexString2Bytes();

            ThinNeo.Transaction claimTran = new ThinNeo.Transaction();
            claimTran.Deserialize(new MemoryStream(txScript));

            JObject contract = ct.getContractState(url, hash);
            //做智能合约的签名
            byte[] iscript = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                sb.EmitPushString("whatever");
                sb.EmitPushNumber(250);
                iscript = sb.ToArray();
            }
            byte[] conbytes = ThinNeo.Debug.DebugTool.HexString2Bytes((string)contract["script"]);
            claimTran.AddWitnessScript(conbytes, iscript);

            var trandata = claimTran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);

            JObject result = tx.sendrawtransaction(url, strtrandata);
            bool re = (bool)result["sendrawtransactionresult"];
            Console.WriteLine("得到的结果是：" + re);
            if (re)
            {
                Console.WriteLine("txid：" + (string)result["txid"]);
                try
                {
                    //处理提取后的utxo,记录每笔utxo原始账户信息
                    new Thread(o =>
                    {
                        proClaimData(mongodbConnStr, mongodbDatabase, claimGas);

                    })
                    { IsBackground = true}
                    .Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("errorMessage：" + (string)result["errorMessage"]);

            }
            return result;
        }

        private void proClaimData(string mongodbConnStr, string mongodbDatabase,JObject claimGas)
        {
            List<ThinNeo.TransactionInput> claimVins = new List<ThinNeo.TransactionInput>();
            foreach (JObject j in (JArray)claimGas["claims"])
            {
                string txid = (string)j["txid"];
                ushort index = (ushort)j["n"];
                try
                {
                    Console.WriteLine("txid:"+txid+"/n:"+index);
                    processSigleTx(mongodbConnStr, mongodbDatabase, txid, index);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public JObject processSigleTx(string mongodbConnStr, string mongodbDatabase, string txid,int n)
        {
            decimal issueGas = 0;
            string findFliter = "{txid:'" + txid + "',n:" + n + ",'asset':'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b'}";
            JArray gasIssueJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
            JObject utxo = (JObject)gasIssueJA[0];
        
            JObject gas = new JObject();
            string used = (string)utxo["used"];
            int start = (int)utxo["createHeight"];
            int end = -1;
            if (used.Length > 0)
            {
                end = (int)utxo["useHeight"] - 1; //转出的这块的gas属于转入地址
            }
            else
            {
                //未花费以目前高度计算
                end = (int)mh.Getdatablockheight(mongodbConnStr, mongodbDatabase).First()["blockDataHeight"];
            }
            int value = (int)utxo["value"];

            //转入的这个块，属于当前地址，由于差额计算方法，需要开始-1
            decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start - 1);

            decimal issueGasInBlock = countGas(start, end);

            issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;

            ushort index = (ushort)utxo["n"];
            string usedTxid = (string)utxo["used"];

            gas.Add("addr", (string)utxo["addr"]);
            gas.Add("txid", txid);
            gas.Add("n", index);
            gas.Add("gas", issueGas);

            string formatGas = DecimalToString(issueGas);

            Console.WriteLine("gas:"+issueGas+"/format gas:"+ formatGas);

            //根据usedTxid查询原始转入地址
            //db.tx.find({"vin.0.txid":{$eq:"0x24b6752cd46aefdf036a86b8e05af7e024d15846e728d24e4a10c9e9ebf09927"}})
            findFliter = "{'vin.0.txid':{$eq:'" + usedTxid + "'}}";
            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
            if (result.Count > 0)
            {
                JObject txOb = (JObject)result[0];
                JArray vouts = (JArray)txOb["vout"];
                if (vouts.Count > 0)
                {
                    JObject voutOb = (JObject)vouts[0];
                    string claimAddr = (string)voutOb["address"];
                    int v = (int)voutOb["value"];

                    findFliter = "{txid:'" + txid+"',n:" + n + "}";
                    JArray array =  mh.GetData(mongodbConnStr, mongodbDatabase, "ProClaimgas", findFliter);
                    if (array.Count > 0)
                    {
                        return gas;
                    }
                    else
                    {
                        var client = new MongoClient(mongodbConnStr);
                        var database = client.GetDatabase(mongodbDatabase);
                        ProClaimgas pro = new ProClaimgas(claimAddr,txid, index, usedTxid, v, issueGas, 1, DateTime.Now, DateTime.Now);
                        var collectionPro = database.GetCollection<ProClaimgas>("ProClaimgas");
                        collectionPro.InsertOne(pro);
                    }

                }
            }
            return gas;

        }

        public JObject processClaimStatus(string mongodbConnStr, string mongodbDatabase, string txid, int n,string trTxid)
        {
            JObject ret = new JObject();
            bool result = false;
            string findFliter = "{txid:'" + txid + "',n:" + n + ",status:1}";
            JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "ProClaimgas", findFliter);
            if (array.Count == 0)
            {
                ret.Add("result", result);
                return ret;
            }

            JObject txOb = (JObject)array[0];
            int status = (int)txOb["status"];
            string claimAddr = (string)txOb["claimAddr"];
            decimal gas = (decimal)txOb["gas"];

            //2:已处理
            if (status == 2)
            {
                ret.Add("result", result);
                return ret;
            }

            //根据trTxid查询交易是否是gas转账
            findFliter = "{txid:'" + trTxid + "'}";
            JArray txs = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
            if (txs.Count > 0)
            {
                txOb = (JObject)txs[0];
                //ContractTransaction
                string type = (string)txOb["type"]; 
                JArray vouts = (JArray)txOb["vout"];

                if (vouts.Count > 0 && type == "ContractTransaction")
                {
                    JObject voutOb = (JObject)vouts[0];
                    string address = (string)voutOb["address"];
                    decimal value = (decimal)voutOb["value"];

                    if(address == claimAddr && gas.CompareTo(value) == 0)
                    {
                        var client = new MongoClient(mongodbConnStr);
                        var database = client.GetDatabase(mongodbDatabase);
                        var coll = database.GetCollection<ProClaimgas>("ProClaimgas");
                        BsonDocument queryBson = BsonDocument.Parse("{txid:'" + txid + "',n:" + n + ",status:1}");
                        List<ProClaimgas> queryBsonList = coll.Find(queryBson).ToList();

                        if (queryBsonList.Count > 0)
                        {
                            ProClaimgas pro = queryBsonList[0];
                            pro.status = 2;
                            pro.pro = DateTime.Now;
                            coll.ReplaceOne(queryBson, pro);
                            result = true;
                        }
                    }
                }

            }
            ret.Add("result", result);
            return ret;
        }

        public JObject lockClaimStatus(string mongodbConnStr, string mongodbDatabase, string txid, int n)
        {
            JObject ret = new JObject();
            bool result = false;
            string findFliter = "{txid:'" + txid + "',status:1}";
            JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "MintGas", findFliter);
            if (array.Count == 0)
            {
                ret.Add("result", result);
                return ret;
            }

            JObject txOb = (JObject)array[0];
            int status = (int)txOb["status"];

            //2:已处理
            if (status == 2)
            {
                ret.Add("result", result);
                return ret;
            }

            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var coll = database.GetCollection<NEP55.MintGas>("MintGas");
            BsonDocument queryBson = BsonDocument.Parse("{txid:'" + txid + "',status:1}");
            List<NEP55.MintGas> queryBsonList = coll.Find(queryBson).ToList();

            if (queryBsonList.Count > 0)
            {
                NEP55.MintGas pro = queryBsonList[0];
                pro.status = 2;
                pro.claimTime = DateTime.Now;

                coll.ReplaceOne(queryBson, pro);
                result = true;
            }
            ret.Add("result", result);
            return ret;
        }

        //public JObject lockClaimStatus(string mongodbConnStr, string mongodbDatabase, string txid, int n)
        //{
        //    JObject ret = new JObject();
        //    bool result = false;
        //    string findFliter = "{txid:'" + txid + "',n:" + n + ",status:1}";
        //    JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "ProClaimgas", findFliter);
        //    if (array.Count == 0)
        //    {
        //        ret.Add("result", result);
        //        return ret;
        //    }

        //    JObject txOb = (JObject)array[0];
        //    int status = (int)txOb["status"];

        //    //2:已处理
        //    if (status == 2)
        //    {
        //        ret.Add("result", result);
        //        return ret;
        //    }

        //    var client = new MongoClient(mongodbConnStr);
        //    var database = client.GetDatabase(mongodbDatabase);
        //    var coll = database.GetCollection<ProClaimgas>("ProClaimgas");
        //    BsonDocument queryBson = BsonDocument.Parse("{txid:'" + txid + "',n:" + n + ",status:1}");
        //    List<ProClaimgas> queryBsonList = coll.Find(queryBson).ToList();

        //    if (queryBsonList.Count > 0)
        //    {
        //        ProClaimgas pro = queryBsonList[0];
        //        pro.status = 2;
        //        pro.pro = DateTime.Now;
        //        coll.ReplaceOne(queryBson, pro);
        //        result = true;
        //    }
        //    ret.Add("result", result);
        //    return ret;
        //}

        internal JArray getClaimGasGroup(string mongodbConnStr, string mongodbDatabase)
        {
            string findFliter = "{}";
            JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "MintGas", findFliter);

            Dictionary<string, decimal> balance = new Dictionary<string, decimal>();
            foreach (JObject j in array)
            {
                if (!balance.ContainsKey((string)j["from"]))
                {
                    balance.Add((string)j["from"], (decimal)j["gas"]);
                }
                else
                {
                    balance[(string)j["from"]] += (decimal)j["gas"];
                }
            }
            JArray balanceJA = new JArray();
            foreach (KeyValuePair<string, decimal> kv in balance)
            {
                JObject j = new JObject();
                j.Add("addr", kv.Key);
                j.Add("gas", kv.Value);
                balanceJA.Add(j);
            }

            return balanceJA;
        }

        internal JObject getSendGas(string mongodbConnStr, string mongodbDatabase,string addr)
        {
            JObject ret = new JObject();
            string findFliter = "{addr:'" + addr + "'}";

            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "address_tx",findFliter);

            Dictionary<string, decimal> balance = new Dictionary<string, decimal>();
            foreach (JObject ob in result) {
                string txid = (string)ob["txid"];
                balance = processGasTransfer(mongodbConnStr,mongodbDatabase,txid, balance);
            }
            decimal gas_value = 0; //所有gas总值
            JArray balanceJA = new JArray();
            foreach (KeyValuePair<string, decimal> kv in balance)
            {
                JObject j = new JObject();
                j.Add("addr", kv.Key);
                j.Add("gas", kv.Value);
                //balanceJA.Add(j);
                gas_value += kv.Value;
                mh.insertOne(mongodbConnStr,mongodbDatabase,"HasSendGas",j);
            }
            ret.Add("gas", gas_value);
            return ret;
        }

        private Dictionary<string, decimal> processGasTransfer(string mongodbConnStr,string mongodbDatabase,string txid,Dictionary<string, decimal> balance)
        {
            string findFliter = "{txid:'" + txid + "',type:'ContractTransaction'}";
            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx",findFliter);

            if (result.Count > 0) {
                JObject ob = (JObject)result[0];
                JArray vouts = (JArray)ob["vout"];
                foreach (JObject vout in vouts) {
                    if (!balance.ContainsKey((string)vout["address"]))
                    {
                        balance.Add((string)vout["address"], (decimal)vout["value"]);
                    }
                    else
                    {
                        balance[(string)vout["address"]] += (decimal)vout["value"];
                    }
                }

            }

            return balance;
            
        }

        internal JObject getGasByStatus(string mongodbConnStr, string mongodbDatabase, int status)
        {
            JObject ret = new JObject();

            string findFliter = "{}";
            if (status > 0)
            {
                findFliter = "{status:" + status + "}";
            }
            JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "MintGas", findFliter);

            decimal gas_value = 0; //所有gas总值
            foreach (JObject mint in array)
            {
                gas_value += (decimal)mint["gas"];
            }
            ret.Add("gas", gas_value);
            return ret;
        }

        internal JObject getGasByAddr(string mongodbConnStr, string mongodbDatabase, string addr, int status)
        {
            JObject ret = new JObject();

            string findFliter = "{from:'" + addr + "'}";
            if (status > 0)
            {
                findFliter = "{from:'" + addr + "',status:" + status + "}";
            }
            JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "MintGas", findFliter);

            decimal gas_value = 0; //所有gas总值
            foreach (JObject mint in array)
            {
                gas_value += (decimal)mint["gas"];
            }
            ret.Add("gas", gas_value);
            return ret;
        }

        public JObject calGasByHeight(string mongodbConnStr, string mongodbDatabase, int start, int end, int value)
        {
            JObject J = new JObject();
           
            J.Add("gas",getCalGas(mongodbConnStr,mongodbDatabase,start,end,value));
            return J;

        }

        private decimal getCalGas(string mongodbConnStr, string mongodbDatabase, int start, int end, int value)
        {
            if (start >= end) return 0;

            if (value <= 0) return 0;

            decimal issueGas = 0;

            decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start - 1);
            decimal issueGasInBlock = countGas(start, end);

            issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;
            return issueGas;
        }

        public void getMintSNEO(string mongodbConnStr, string mongodbDatabase, string sneoAddr)
        {
            //db.tx.find({"vout.0.address":{$eq:"ASmnUjHydp5z2eDjUWnkHmQbtpm6TjMUPP"},"type":"InvocationTransaction"})
            string findFliter = "{'vout.0.address':{$eq:'" + sneoAddr + "'},'type':'InvocationTransaction'}";
            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);

            foreach (JObject txOb in result) {
                int blockindx = (int)txOb["blockindex"];
                JArray vins = (JArray)txOb["vin"];
                string txid = (string)txOb["txid"];
                JArray vouts = (JArray)txOb["vout"];
            
                JObject valOb = (JObject)vouts[0];
                int value = (int)valOb["value"];

                if (vins.Count > 0)
                {
                    //查出转入地址的交易
                    JObject vinOb = (JObject)vins[0];
                    txid = (string)vinOb["txid"];
                    int n = (int)vinOb["vout"];

                    findFliter = "{txid:'" + txid + "'}";
                    JArray array = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                    if (array.Count > 0)
                    {
                        JObject txOb2 = (JObject)array[0];
                        vouts = (JArray)txOb2["vout"];
                        if (vouts.Count > 0)
                        {
                            //查出转入地址的交易
                            JObject voutOb = (JObject)vouts[n];
                            string from = (string)voutOb["address"];

                            if(!getMintSNEOByTxid(mongodbConnStr, mongodbDatabase, txid))
                            {
                                var client = new MongoClient(mongodbConnStr);
                                var database = client.GetDatabase(mongodbDatabase);
                                NEP55.MintSNEO mint = new NEP55.MintSNEO(blockindx, txid, from, sneoAddr, value, GetBlockTime(mongodbConnStr, mongodbDatabase, blockindx));
                                var collectionPro = database.GetCollection<NEP55.MintSNEO>("MintSNEO");
                                collectionPro.InsertOne(mint);
                            }
                        }

                    }
                }
            }
            
        }

        public void getRefundGas(string mongodbConnStr, string mongodbDatabase, string sneoAddr)
        {
            //首先查出所有refund记录
            string findFliter = "{}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "operated4C", findFliter);
            Console.WriteLine("***operated4C***");
            foreach (JObject ob in arrays)
            {
                string addr = (string)ob["addr"];
                string txid = (string)ob["txid"];
                int n = (int)ob["n"];
                int refundVal = (int)ob["value"];
                int endHeigth = (int)ob["blockindex"];

                //查询MintGAS是否存在，如果有就不处理
                if (!getMintGasByTxid(mongodbConnStr, mongodbDatabase, txid)) {
                    Console.WriteLine("***addr***" + addr + "/txid***" + txid);
                    //取值没问题才进行入库
                    int startHeight = getStartHeight(mongodbConnStr,mongodbDatabase,refundVal,addr);
                    Console.WriteLine("***startHeight***"+ startHeight);
                    Thread.Sleep(50);
                    if (startHeight > 0) {
                        var client = new MongoClient(mongodbConnStr);
                        var database = client.GetDatabase(mongodbDatabase);

                        //计算gas
                        decimal gas = getCalGas(mongodbConnStr,mongodbDatabase,startHeight,endHeigth,refundVal);
                        NEP55.MintGas mint = new NEP55.MintGas(startHeight,endHeigth,txid,addr,refundVal,gas,DateTime.Now);
                        var collectionPro = database.GetCollection<NEP55.MintGas>("MintGas");
                        collectionPro.InsertOne(mint);
                    }
                }
            }

        }

        private int getStartHeight(string mongodbConnStr, string mongodbDatabase, int refundVal,string addr)
        {
            //地址下面所有未使用的兑换记录
            JArray mints = getMintSNEOByAddr(mongodbConnStr,mongodbDatabase,addr);

            if (mints.Count == 0) return 0;

            //默认第一个就是开始高度
            JObject firstOb = (JObject)mints[0];
            int remainTotal = 0;
            
            Console.WriteLine("mintsneo size:" + mints.Count);
            //记录使用被匹配的兑换记录
            List<NEP5.MintSNEOTemp> temps = new List<NEP5.MintSNEOTemp>();
            foreach (JObject mint in mints) {
                int remainVal = (int)mint["remainVal"];
                string txid = (string)mint["txid"];
                int blockindex = (int)mint["blockindex"];

                remainTotal = remainTotal + remainVal;

                int result = remainTotal - refundVal;
                if (result <= 0)
                {
                    NEP5.MintSNEOTemp temp = new NEP5.MintSNEOTemp(addr, txid, 0);
                    temps.Add(temp);
                }
                else {
                    NEP5.MintSNEOTemp temp = new NEP5.MintSNEOTemp(addr, txid, result);
                    temps.Add(temp);
                    break;
                }
            }

            //更新MintSNEO记录
            foreach (NEP5.MintSNEOTemp temp in temps)
            {
                string txid = temp.txid;
                int remainVal = temp.remainVal;

                Console.WriteLine("txid:"+txid+ "/remainVal:"+ remainVal);
                var client = new MongoClient(mongodbConnStr);
                var database = client.GetDatabase(mongodbDatabase);
                var coll = database.GetCollection<BsonDocument>("MintSNEO");
                BsonDocument queryBson = BsonDocument.Parse("{from:'" + addr + "',txid:'" + txid + "'}");

                var collBson = database.GetCollection<NEP55.MintSNEO>("MintSNEO");
                List<NEP55.MintSNEO> query = collBson.Find(queryBson).ToList();
                
                NEP55.MintSNEO oper = query[0];
                oper.remainVal = remainVal;

                collBson.ReplaceOne(queryBson, oper);
            }

            return (int)firstOb["blockindex"];
        }


        private Boolean getMintSNEOByTxid(string mongodbConnStr, string mongodbDatabase,string txid)
        {
            string findFliter = "{txid:'"+txid+"'}";
            long num = mh.GetDataCount(mongodbConnStr, mongodbDatabase, "MintSNEO", findFliter);
            if (num > 0) return true;
            return false;
        }

        private JObject getSinMintSNEOByTxid(string mongodbConnStr, string mongodbDatabase, string txid)
        {
            string findFliter = "{txid:'" + txid + "'}";
            JArray results = mh.GetData(mongodbConnStr, mongodbDatabase, "MintSNEO", findFliter);
            
            return (JObject)results[0];
        }

        private Boolean getMintGasByTxid(string mongodbConnStr, string mongodbDatabase, string txid)
        {
            string findFliter = "{txid:'"+txid+"'}";
            long num = mh.GetDataCount(mongodbConnStr, mongodbDatabase, "MintGas", findFliter);
            if (num > 0) return true;
            return false;
        }

        private JArray getMintSNEOByAddr(string mongodbConnStr, string mongodbDatabase, string addr)
        {
            //根据地址查询remainVal大于0，默认按照升序排序
            string findFliter = "{'from':'" + addr + "','remainVal':{ $gt:'0'}}";
            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "MintSNEO", findFliter);
            return result;

        }

        private static DateTime GetBlockTime(string mongodbConnStr, string mongodbDatabase,int blockindex)
        {
            //获取block时间（本地时区时区）
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collBlock = database.GetCollection<BsonDocument>("block");
            var queryBlock = collBlock.Find("{index:" + blockindex + "}").Project("{time:1}").ToList()[0];
            int blockTimeTS = queryBlock["time"].AsInt32;
            DateTime blockTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local).AddSeconds(blockTimeTS);

            client = null;

            return blockTime;
        }

        private string DecimalToString(decimal d)
        {
            return d.ToString("#0.########");
        }
    }
}
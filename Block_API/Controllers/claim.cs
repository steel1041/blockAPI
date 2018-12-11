using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NEO_Block_API.lib;

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

                    decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start);
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

                    decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start);
                    decimal issueGasInBlock = countGas(start, end);

                    issueGas += (issueSysfee + issueGasInBlock) / 100000000 * value;
                }

                J.Add("gas", issueGas);
                J.Add("claims", gasIssueJA);
                J.Add("count", utxoCount);
            }

            return J;
        }

        public JObject getClaimGasByTx(string mongodbConnStr, string mongodbDatabase, string txid)
        {
            decimal issueGas = 0;

            string findFliter = "{txid:'" + txid + "','asset':'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b'}";

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

                decimal issueSysfee = mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, end) - mh.GetTotalSysFeeByBlock(mongodbConnStr, mongodbDatabase, start);
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

        public void claimContract(string mongodbConnStr, string mongodbDatabase, string conAddr, string addrClaim, string url, string hash)
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
        }
    }
}
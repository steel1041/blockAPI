using NEO_Block_API.lib;
using NEO_Block_API.RPC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Block_API.Controllers;
using ThinNeo;

namespace NEO_Block_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        private string mongodbConnStr { get; set; }
        private string mongodbDatabase { get; set; }
        private string neoCliJsonRPCUrl { get; set; }
        private string neoCliJsonRPCUrlLocal { get; set; }
        private string neoRpcUrl_mainnet { get; set; }

        public string hashSDUSD { get; set; }
        public string hashSAR4C { get; set; }
        public string hashSAR4B { get; set; }
        public string hashSNEO { get; set; }
        public string hashORACLE { get; set; }
        public string addrSAR4C { get; set; }
        public string oldAddrSAR4C { get; set; }
        public string hashSDS { get; set; }
        public string hashTokenized { get; set; }
        public string wif { get; set; }

        httpHelper hh = new httpHelper();
        mongoHelper mh = new mongoHelper();
        Transaction tx = new Transaction();
        Contract ct = new Contract();
        Claim claim = new Claim();
        Business bu = new Business();
        Transaction tr = new Transaction();

        public Api(string node) {
            netnode = node;
            switch (netnode) {
                case "testnet":
                    mongodbConnStr = mh.mongodbConnStr_testnet;
                    mongodbDatabase = mh.mongodbDatabase_testnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_testnet;
                    neoCliJsonRPCUrlLocal = mh.neoCliJsonRPCUrl_local_testnet;
                    hashSDUSD = bu.hashSDUSD_testnet;
                    hashSAR4C = bu.hashSAR4C_testnet;
                    hashSAR4B = bu.hashSAR4B_testnet;
                    hashSNEO = bu.hashSNEO_testnet;
                    hashORACLE = bu.hashORACLE_testnet;
                    addrSAR4C = bu.addrSAR4C_testnet;
                    oldAddrSAR4C = bu.oldAddrSAR4C_testnet;
                    hashSDS = bu.hashSDS_testnet;
                    hashTokenized = bu.hashTokenized_testnet;
                    wif = bu.wif_testnet;
                    break;
                case "mainnet":
                    mongodbConnStr = mh.mongodbConnStr_mainnet;
                    mongodbDatabase = mh.mongodbDatabase_mainnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_mainnet;
                    neoCliJsonRPCUrlLocal = mh.neoCliJsonRPCUrl_local_mainnet;
                    neoRpcUrl_mainnet = mh.neoRpcUrl_mainnet;
                    hashSDUSD = bu.hashSDUSD_mainnet;
                    hashSAR4C = bu.hashSAR4C_mainnet;
                    hashSAR4B = bu.hashSAR4B_mainnet;
                    hashSNEO = bu.hashSNEO_mainnet;
                    hashORACLE = bu.hashORACLE_mainnet;
                    addrSAR4C = bu.addrSAR4C_mainnet;
                    oldAddrSAR4C = bu.oldAddrSAR4C_mainnet;
                    hashSDS = bu.hashSDS_mainnet;
                    hashTokenized = bu.hashTokenized_mainnet;
                    wif = bu.wif_mainnet;
                    break;
                case "swnet":
                    mongodbConnStr = mh.mongodbConnStr_swnet;
                    mongodbDatabase = mh.mongodbDatabase_swnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_swnet;
                    break;
                case "pri":
                    mongodbConnStr = mh.mongodbConnStr_pri;
                    mongodbDatabase = mh.mongodbDatabase_pri;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_pri;
                    neoCliJsonRPCUrlLocal = mh.neoCliJsonRPCUrl_local_pri;
                    hashSDUSD = bu.hashSDUSD_prinet;
                    hashSAR4C = bu.hashSAR4C_prinet;
                    hashSAR4B = bu.hashSAR4B_prinet;
                    hashSNEO = bu.hashSNEO_prinet;
                    hashORACLE = bu.hashORACLE_prinet;
                    addrSAR4C = bu.addrSAR4C_prinet;
                    oldAddrSAR4C = bu.oldAddrSAR4C_prinet;
                    hashSDS = bu.hashSDS_prinet;
                    hashTokenized = bu.hashTokenized_prinet;
                    wif = bu.wif_prinet;
                    break;
            }
        }

        private JArray getJAbyKV(string key, object value)
        {
            return  new JArray
                        {
                            new JObject
                            {
                                {
                                    key,
                                    value.ToString()
                                }
                            }
                        };
        }

        private JArray getJAbyJ(JObject J)
        {
            return new JArray
                        {
                            J
                        };
        }

        public object getRes(JsonRPCrequest req,string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                switch (req.method)
                {
                    case "getnodetype":
                        JArray JA = new JArray
                        {
                            new JObject {
                                { "nodeType",netnode }
                            }
                        };
                        result = JA;
                        break;
                    case "getcliversion":
                        result = getJAbyKV("cliversion", (string)JObject.Parse(hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getversion','params':[],'id':1}", System.Text.Encoding.UTF8, 1))["result"]["useragent"]);
                        break;
                    case "getclirawmempool":
                        JObject rawmempoolJ = new JObject();
                        rawmempoolJ.Add("clirawmempool", JObject.Parse(hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getrawmempool','params':[],'id':1}", System.Text.Encoding.UTF8, 1))["result"]);
                        result = getJAbyJ(rawmempoolJ);
                        break;
                    case "getdatablockheight":
                        result = mh.Getdatablockheight(mongodbConnStr, mongodbDatabase);
                        break;
                    case "getblockcount":
                        result = getJAbyKV("blockcount", (long)(mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{counter:'block'}")[0]["lastBlockindex"]) + 1);
                        break;
                    case "getngdblockcount":
                        result = getJAbyJ(bu.getNgdBlockcount(neoRpcUrl_mainnet));
                        break;
                    case "getcliblockcount":
                        var resp = hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getblockcount','params':[],'id':1}", System.Text.Encoding.UTF8, 1);

                        string cliResultStr = (string)JObject.Parse(resp)["result"];
                        result = getJAbyKV("cliblockcount", cliResultStr);
                        break;
                    case "claimContract":
                        if (req.@params.Count() > 0)
                        {
                            string contractAddr = req.@params[0].ToString();
                            string claimAddr = req.@params[1].ToString();
                            string sarHash = req.@params[2].ToString();
                            result = getJAbyJ(claim.claimContract(mongodbConnStr, mongodbDatabase, contractAddr, claimAddr, neoCliJsonRPCUrl, sarHash));
                        }
                        break;
                    case "getProcessClaim":
                        if (req.@params.Count() > 0)
                        { 
                            int status = int.Parse(req.@params[0].ToString());
                            result = claim.getMintGasData(mongodbConnStr,mongodbDatabase,status);
                            //result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "ProClaimgas", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                         }
                        break;
                    case "processClaimStatus":
                        if (req.@params.Count() > 0)
                        {
                            result = getJAbyJ(claim.processClaimStatus(mongodbConnStr, mongodbDatabase, req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), req.@params[2].ToString()));
                        }
                        break;
                    case "lockClaimStatus":
                        if (req.@params.Count() > 0)
                        {
                            result = getJAbyJ(claim.lockClaimStatus(mongodbConnStr, mongodbDatabase, req.@params[0].ToString(), int.Parse(req.@params[1].ToString())));
                        }
                        break;
                    case "gettxcount":
                        //resultStr = "[{txcount:" + mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx") + "}]";
                        //result = getJAbyKV("txcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx"));
                        findFliter = "{}";
                        if (req.@params.Count() > 0)
                        {
                            string type = req.@params[0].ToString();
                            if (type != null && type != string.Empty)
                            {
                                findFliter = "{type:\"" + type + "\"}";
                            }
                        }
                        result = getJAbyKV("txcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx", findFliter));
                        break;
                    case "getaddrcount":
                        //resultStr = "[{addrcount:" + mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address") + "}]";
                        result = getJAbyKV("addrcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address"));
                        break;
                    case "getblock":
                        findFliter = "{index:" + req.@params[0] + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter);
                        break;
                    case "getblocktime":
                        findFliter = "{index:" + req.@params[0] + "}";
                        var time = (Int32)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["time"];
                        result = getJAbyKV("time", time);
                        break;
                    case "getblocks":
                        sortStr = "{index:-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "block", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getrawtransaction":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        break;
                    case "getrawtransactions":
                        sortStr = "{blockindex:-1,txid:-1}";
                        findFliter = "{}";
                        if (req.@params.Count() > 2)
                        {
                            string txType = req.@params[2].ToString();

                            if (txType != null && txType != string.Empty)
                            { findFliter = "{type:'" + txType + "'}"; }
                        }
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        break;
                    case "getaddrs":
                        sortStr = "{'lastuse.blockindex' : -1,'lastuse.txid' : -1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getaddr":
                        string addr = req.@params[0].ToString();
                        findFliter = "{addr:'" + addr + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "address", findFliter);
                        break;
                    case "getaddresstxs":
                        string findBson = "{'addr':'" + req.@params[0].ToString() + "'}";
                        sortStr = "{'blockindex' : -1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findBson);
                        break;
                    case "getasset":
                        findFliter = "{id:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "asset", findFliter);
                        break;
                    case "getallasset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "asset", findFliter);
                        break;
                    case "getfulllog":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "fulllog", findFliter);
                        break;
                    case "getnotify":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "notify", findFliter);
                        break;
                    case "getutxo":
                        if (req.@params.Count() == 1)
                        {
                            findFliter = "{addr:'" + req.@params[0] + "',used:''}";
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
                        }
                        else if (req.@params.Count() == 2)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                findFliter = "{addr:'" + req.@params[0] + "'}";
                            }
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            findFliter = "{addr:'" + req.@params[0] + "',used:''}";
                            sortStr = "{'createHeight':1,'txid':1,'n':1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "utxo", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                findFliter = "{addr:'" + req.@params[0] + "'}";
                            }
                            sortStr = "{'createHeight':1,'txid':1,'n':1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "utxo", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getutxocount":
                        addr = req.@params[0].ToString();
                        if (addr != null && addr != string.Empty)
                        {
                            findFliter = "{addr:\"" + addr + "\"}";
                        }
                        result = getJAbyKV("utxocount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "utxo", findFliter));
                        break;
                    case "getutxostopay":
                        string address = (string)req.@params[0];
                        string assetID = ((string)req.@params[1]).formatHexStr();
                        decimal amount = decimal.Parse(req.@params[2].ToString(),NumberStyles.Float);
                        bool isBigFirst = false; //默认先用小的。

                        if (req.@params.Count() == 4)
                        {
                            if ((Int64)req.@params[3] == 1)
                            {
                                isBigFirst = true;//加可选参数可以先用大的。
                            }
                        }

                        findFliter = "{addr:'" + address + "',used:''}";
                        JArray utxoJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        result = tx.getUtxo2Pay(utxoJA, address, assetID, amount, isBigFirst);
                        break;
                    case "getclaimgas":
                        JObject claimsJ = new JObject();
                        if (req.@params.Count() == 1)
                        {
                            claimsJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, req.@params[0].ToString());
                        };
                        if (req.@params.Count() == 2)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                claimsJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, req.@params[0].ToString(), false);
                            }
                        }
                        result = getJAbyJ(claimsJ);
                        break;
                    case "getClaimGasByTx":
                        claimsJ = new JObject();
                        if (req.@params.Count() == 2)
                        {
                            claimsJ = claim.getClaimGasByTx(mongodbConnStr, mongodbDatabase, ((string)req.@params[0]).formatHexStr(), int.Parse(req.@params[1].ToString()));
                        };
                        result = getJAbyJ(claimsJ);
                        break;
                    case "calGasByHeight":
                        claimsJ = new JObject();
                        if (req.@params.Count() == 3)
                        {
                            claimsJ = claim.calGasByHeight(mongodbConnStr, mongodbDatabase, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        };
                        result = getJAbyJ(claimsJ);
                        break;
                    case "getMintSNEO":
                        addr = (string)req.@params[0];
                        claim.getMintSNEO(mongodbConnStr,mongodbDatabase, addr);
                        result = getJAbyKV("result","true");
                        break;
                    case "getRefundGas":
                        addr = (string)req.@params[0];
                        claim.getRefundGas(mongodbConnStr, mongodbDatabase, addr);
                        result = getJAbyKV("result", "true");
                        break;
                    case "getGasByAddr":
                        if (req.@params.Count() == 2)
                        {
                            //根据状态地址的gas
                            addr = req.@params[0].ToString();
                            int status = int.Parse(req.@params[1].ToString());
                            result = getJAbyJ(claim.getGasByAddr(mongodbConnStr, mongodbDatabase,addr, status));
                        }
                        else if (req.@params.Count() == 1)
                        {
                            //地址的总计gas
                            addr = req.@params[0].ToString();
                            result = getJAbyJ(claim.getGasByAddr(mongodbConnStr, mongodbDatabase,addr, 0));
                        }
                        else {
                            //地址的所有gas，按照地址分组
                            result = claim.getClaimGasGroup(mongodbConnStr, mongodbDatabase);
                        }
                        break;
                    case "getGasByStatus":
                        if (req.@params.Count() == 1)
                        {
                            //根据状态地址的gas
                            int status = int.Parse(req.@params[0].ToString());
                            result = getJAbyJ(claim.getGasByStatus(mongodbConnStr, mongodbDatabase,status));
                        }
                        else
                        {
                            //地址的总计gas
                            addr = req.@params[0].ToString();
                            result = getJAbyJ(claim.getGasByStatus(mongodbConnStr, mongodbDatabase, 0));
                        }
                        break;
                    case "getSendGas":
                        addr = req.@params[0].ToString();
                        result = getJAbyJ(claim.getSendGas(mongodbConnStr, mongodbDatabase, addr));
                        break;
                    case "getclaimtxhex":
                        string addrClaim = (string)req.@params[0];
                        JObject claimgasJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, addrClaim);
                        if (claimgasJ["errorCode"] != null)
                        {
                            result = getJAbyJ(claimgasJ);
                        }
                        else
                        {
                            result = getJAbyKV("claimtxhex", tx.getClaimTxHex(addrClaim, claimgasJ));
                        }
                        break;
                    case "getbalance":
                        addr = (string)req.@params[0];
                        result = bu.getGlobalBalance(mongodbConnStr, mongodbDatabase,addr);
                        break;
                    case "getcontractscript":
                        findFliter = "{hash:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mh.mongodbConnStr_NeonOnline, mh.mongodbDatabase_NeonOnline, "contractWarehouse", findFliter);
                        break;
                    case "gettransfertxhex":
                        string addrOut = (string)req.@params[0];
                        findFliter = "{addr:'" + addrOut + "',used:''}";
                        JArray outputJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        result = getJAbyKV("transfertxhex", tx.getTransferTxHex(outputJA, (string)req.@params[0], (string)req.@params[1], (string)req.@params[2], decimal.Parse(req.@params[3].ToString(),NumberStyles.Float)));
                        break;
                    case "sendtxplussign":
                        result = getJAbyJ(tx.sendTxPlusSign(neoCliJsonRPCUrl, (string)req.@params[0], (string)req.@params[1], (string)req.@params[2]));
                        break;
                    case "verifytxsign":
                        result = getJAbyKV("sign", tx.verifyTxSign((string)req.@params[0], (string)req.@params[1]));
                        break;
                    case "sendrawtransaction":
                        result = getJAbyJ(tx.sendrawtransaction(neoCliJsonRPCUrl, (string)req.@params[0]));
                        break;
                    case "getcontractstate":
                        result = getJAbyJ(ct.getContractState(neoCliJsonRPCUrl, (string)req.@params[0]));

                        break;
                    case "invokescript":
                        result = getJAbyJ(ct.invokeScript(neoCliJsonRPCUrl, (string)req.@params[0]));

                        break;
                    case "callcontractfortest":
                        result = getJAbyJ(ct.callContractForTest(neoCliJsonRPCUrl, new List<string> { (string)req.@params[0] }, new JArray() { (JArray)req.@params[1] }));

                        break;
                    case "callnep55common":
                        result = getJAbyJ(ct.callnep55common(neoCliJsonRPCUrl,new List<string> { (string)req.@params[0]},new JArray() { (JArray)req.@params[1]}));
                        break;
                    case "publishcontractfortest":
                        result = getJAbyJ(ct.publishContractForTest(neoCliJsonRPCUrl, (string)req.@params[0], (JObject)req.@params[1]));
                        break;
                    case "getinvoketxhex":
                        string addrPayFee = (string)req.@params[0];
                        findFliter = "{addr:'" + addrPayFee + "',used:''}";
                        JArray outputJAPayFee = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        string invokeScript = (string)req.@params[1];
                        decimal invokeScriptFee = decimal.Parse(req.@params[2].ToString(),NumberStyles.Float);

                        result = getJAbyKV("invoketxhex", tx.getInvokeTxHex(outputJAPayFee, addrPayFee, invokeScript, invokeScriptFee));
                        break;
                    case "getstorage":
                        result = getJAbyJ(ct.getStorage(neoCliJsonRPCUrl, (string)req.@params[0], (string)req.@params[1]));

                        break;
                    case "setcontractscript":
                        JObject J = JObject.Parse((string)req.@params[0]);
                        string hash = (string)J["hash"];

                        J.Add("requestIP", reqAddr);

                        mh.InsertOneDataByCheckKey(mh.mongodbConnStr_NeonOnline, mh.mongodbDatabase_NeonOnline, "contractWarehouse", J, "hash", hash);
                        result = getJAbyKV("isSetSuccess", true);

                        break;
                    case "getnep5balanceofaddress":
                        string NEP5scripthash = (string)req.@params[0];
                        string NEP5address = (string)req.@params[1];
                        byte[] NEP5addrHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(NEP5address);
                        string NEP5addrHashHex = ThinNeo.Helper.Bytes2HexString(NEP5addrHash.Reverse().ToArray());
                        JObject NEP5balanceOfJ = ct.callContractForTest(neoCliJsonRPCUrl,new List<string>{ NEP5scripthash }, new JArray() { JArray.Parse("['(str)balanceOf',['(hex)" + NEP5addrHashHex + "']]") });
                        string balanceStr = (string)((JArray)NEP5balanceOfJ["stack"])[0]["value"];
                        string balanceType = (string)((JArray)NEP5balanceOfJ["stack"])[0]["type"];

                        string balanceBigint = "0";

                        if (balanceStr != string.Empty)
                        {
                            //获取NEP5资产信息，获取精度
                            NEP5.Asset NEP5asset = new NEP5.Asset(mongodbConnStr, mongodbDatabase, NEP5scripthash);

                            balanceBigint = NEP5.getNumStrFromStr(balanceType,balanceStr, NEP5asset.decimals);
                        }

                        result = getJAbyKV("nep5balance", balanceBigint);
                        break;
                    case "getallnep5assetofaddress":
                        string NEP5addr = (string)req.@params[0];
                        bool isNeedBalance = false;
                        if (req.@params.Count() > 1)
                        {
                            isNeedBalance = ((Int64)req.@params[1] == 1) ? true : false;
                        }

                        result = bu.getNEP5Balance(mongodbConnStr,mongodbDatabase,neoCliJsonRPCUrl,NEP5addr,isNeedBalance);
                        
                        break;
                    case "getallnep55assetofaddress":
                        string NEP55addr = (string)req.@params[0];
                        string NEP55asset = (string)req.@params[1];

                        isNeedBalance = false;
                        if (req.@params.Count() > 2)
                        {
                            isNeedBalance = ((Int64)req.@params[2] == 1) ? true : false;
                        }

                        result = bu.getNEP55Balance(mongodbConnStr,mongodbDatabase,neoCliJsonRPCUrl,NEP55addr,NEP55asset,isNeedBalance);
                        
                        break;
                    case "getnep5asset":
                        findFliter = "{assetid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findFliter);
                        break;
                    case "getallnep5asset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findFliter);
                        break;
                    case "getnep5transferbytxid":
                        string txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + txid + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    case "getnep5transferbyaddresscount":
                        findFliter = "{}";
                        string NEP5transferAddress = string.Empty;
                        if (req.@params.Count() == 1)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]}";
                        }
                        else if (req.@params.Count() == 2)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            string asset = (string)req.@params[1];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]," + "asset:'" + asset + "'}";
                        }
                        result = getJAbyKV("transfercount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter));
                        break;
                    case "getnep5transferbyaddress":
                        sortStr = "{'blockindex':-1}";
                        findFliter = "{}";
                        if (req.@params.Count() == 3)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            string asset = (string)req.@params[1];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]," + "asset:'" + asset + "'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getnep5transferbyaddressSimple":
                        sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        NEP5transferAddress = (string)req.@params[0];
                        string NEP5transferAddressType = (string)req.@params[1];
                        findFliter = "{'" + NEP5transferAddressType + "':'" + NEP5transferAddress + "'}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        break;
                    case "getnep5transfers":
                        sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getnep5transfersbyasset":
                        string str_asset = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{asset:'" + str_asset + "'}";
                        //sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        sortStr = "{}";
                        if (req.@params.Count() == 3)
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        else
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    //获取所有nep55资产
                    case "getnep55asset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP55asset", findFliter);
                        break;
                    case "getnep55transfersbyname":
                        string nep55_name = "";
                 
                        if (req.@params.Count() == 3)
                        {
                            nep55_name = (string)req.@params[0];
                            findFliter = "{name:'" + nep55_name + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 4) {
                            nep55_name = (string)req.@params[0];
                            addr = (string)req.@params[1];
                            findFliter = "{name:'" + nep55_name + "',from:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "transferSAR", findFliter);
                        break;
                    case "getnep55transfersbyaddresscount":
                        addr = (string)req.@params[0];
                        findFliter = "{$or:[{'from':'" + addr + "'},{'to':'" + addr + "'}]}";

                        result = getJAbyKV("transfercount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "transferSAR", findFliter));
                        break;
                    case "getnep55transfersbyaddress":
                        addr = (string)req.@params[0];
                        findFliter = "{$or:[{'from':'" + addr + "'},{'to':'" + addr + "'}]}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
    
                        break;
                    case "getSAR4BByName":
                        string nep55_asset = ((string)req.@params[0]).formatHexStr();
                        nep55_name = (string)req.@params[1];
                        findFliter = "{asset:'"+ nep55_asset + "',name:'"+nep55_name+"',status:1}";
                        sortStr ="{}";
                        result = mh.GetData(mongodbConnStr,mongodbDatabase,"SAR4B",findFliter);

                        break;
                    //根据用户名获取所有代币名称
                    case "getnep55nameByaddr":
                        if (req.@params.Count() == 1) {
                            addr = (string)req.@params[0];
                            findFliter = "{to:'" + addr + "'}";
                            sortStr = "{}";
                            JArray transfers = mh.GetData(mongodbConnStr,mongodbDatabase, "transferSAR", findFliter);

                            List<string> list = new List<string>();
                            JObject nameResult = null;

                            foreach (JObject tfJ in transfers)
                            {
                                list.Add((string)tfJ["name"]);
                            }

                            List<string> names = list.Distinct().ToList();
                      
                            string strs = "";
                            if (names.Count > 0)
                            {
                                foreach (string name in names)
                                {
                                    strs = name + "," + strs;
                                }
                                strs = strs.Substring(0,strs.Length-1);
                            }

                            nameResult = new JObject();
                            nameResult["name"] = strs;
                            result = new JArray() { nameResult };
                        }
                        break;
                    case "getsarOperatedByaddr":
                        if (req.@params.Count() == 4)
                        {
                            string sarTxid = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{sarTxid:'" + sarTxid + "',addr:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "operatedSAR", findFliter);
                        }
                        break;
                    case "getsar4COperatedByaddr":
                        if (req.@params.Count() == 4)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',type:" + int.Parse(req.@params[1].ToString()) + "}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsar4COperatedBySAR":
                        if (req.@params.Count() == 3)
                        {
                            string sarTxid = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{sarTxid:'" + sarTxid + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getOperated4CByaddr":
                        if (req.@params.Count() == 4)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',type:" + int.Parse(req.@params[1].ToString()) + ",status:1}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',status:1}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsarListByType":
                        findFliter = "{type:" + int.Parse(req.@params[0].ToString()) + "}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        break;
                   
                    case "getsar4bcount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            string status = req.@params[0].ToString();
                            findFliter = "{status:" + status + "}";
                        }
                        else if (req.@params.Count() == 2)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();
                            findFliter = "{status:" + status + ",asset:'" + assetId + "'}";
                        }
                        result = getJAbyKV("sar4bcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter));
                        break;
                    case "getsar4BListByType":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";

                        if (req.@params.Count() == 3)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        if (req.@params.Count() == 4)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();

                            findFliter = "{status:" + status + ",asset:'" + assetId + "'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                        {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsar4BDetailList":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        if (req.@params.Count() == 3)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                            result = bu.processSAR4BDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4B, hashORACLE);
                        }
                        if (req.@params.Count() == 4)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();

                            findFliter = "{status:" + status + ",asset:'" + assetId + "'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                            result = bu.processSAR4BDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4B, hashORACLE);
                        }
                        else
                        {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                            result = bu.processSAR4BDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4B, hashORACLE);
                        }
                        break;
                    case "getsar4BDetailByAdd":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        if (req.@params.Count() == 2)
                        {
                            addr = req.@params[0].ToString();
                            //string assetId = req.@params[1].ToString();

                            //findFliter = "{status:1,addr:'" + addr + "',asset:'" + assetId + "'}";
                            //result = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter);
                            result = bu.processSingleSAR4BDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, addr,hashSAR4B, hashORACLE);
                        }
                        break;
                    case "getsar4ccount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            string status = req.@params[0].ToString();
                            findFliter = "{status:" + status + "}";
                        }
                        else if (req.@params.Count() == 2)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();
                            findFliter = "{status:" + status + ",asset:'" + assetId + "'}";
                        }

                        result = getJAbyKV("sar4ccount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "SAR4C", findFliter));
                        break;
                    case "getsar4CListByType":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";

                        if (req.@params.Count() == 3)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();

                            findFliter = "{status:" + status + ",asset:'"+assetId+"'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsar4CDetailList":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";

                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        if (req.@params.Count() == 3)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                            result = bu.processSARDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C, hashORACLE);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();

                            findFliter = "{status:" + status + ",asset:'" + assetId + "'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                            result = bu.processSARDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C, hashORACLE);

                        }
                        else
                        {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                            result = bu.processSARDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C, hashORACLE);
                        }
                        break;
                    case "getsar4CDetailByAdd":
                        findFliter = "{}";
                        sortStr = "{'blockindex':1}";

                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        if (req.@params.Count() == 2)
                        {
                            addr = req.@params[0].ToString();
                            string assetId = ((string)req.@params[1]).formatHexStr();

                            findFliter = "{status:1,addr:'" + addr + "',asset:'"+assetId+"'}";
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4C",findFliter);
                            result = bu.processSARDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C, hashORACLE);
                        }
                        break;
                    case "getOracleOperated":
                        if (req.@params.Count() == 5)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];

                            findFliter = "{asset:'" + assetID +"',addr:'"+ addr+"',key:'" + req.@params[2].ToString() + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[3].ToString()), int.Parse(req.@params[4].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];

                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getnep5count":
                        findFliter = "{}";
                        if (req.@params.Count() == 2)
                        {
                            string key = (string)req.@params[0];
                            string value = (string)req.@params[1];
                            findFliter = "{\"" + key + "\":\"" + value + "\"}";
                        }
                        result = getJAbyKV("nep5count", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter));
                        break;
                    case "getnep5transferbyblockindex":
                        Int64 blockindex = (Int64)req.@params[0];
                        findFliter = "{blockindex:" + blockindex + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    case "getaddresstxbyblockindex":
                        blockindex = (Int64)req.@params[0];
                        findFliter = "{blockindex:" + blockindex + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "address_tx", findFliter);
                        break;
                    case "gettxinfo":
                        txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + (txid).formatHexStr() + "'}";
                        JArray JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        JObject JOTx = (JObject)JATx[0];
                        var heightforblock = (int)JOTx["blockindex"];
                        var indexforblock = -1;
                        findFliter = "{index:" + heightforblock + "}";
                        result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                        for (var i = 0; i < result.Count; i++)
                        {
                            JObject Jo = (JObject)result[i];
                            if (txid == (string)Jo["txid"])
                            {
                                indexforblock = i;
                            }
                        }
                        JObject JOresult = new JObject();
                        JOresult["heightforblock"] = heightforblock;
                        JOresult["indexforblock"] = indexforblock;
                        result = new JArray() { JOresult };
                        break;
                    case "uxtoinfo":
                        var starttxid = ((string)req.@params[0]).formatHexStr();
                        var voutN = (Int64)req.@params[1];

                        findFliter = "{txid:'" + (starttxid).formatHexStr() + "'}";
                        JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        JOTx = (JObject)JATx[0];
                        int starttxblockheight = (int)JOTx["blockindex"];
                        int starttxblockindex = -1;
                        findFliter = "{index:" + starttxblockheight + "}";
                        result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                        for (var i = 0; i < result.Count; i++)
                        {
                            JObject Jo = (JObject)result[i];
                            if (starttxid == (string)Jo["txid"])
                            {
                                starttxblockindex = i;
                            }
                        }
                        //根据txid和n获取utxo信息
                        findFliter = "{txid:\"" + starttxid + "\",n:" + voutN + "}";
                        var endtxid = (string)mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter)[0]["used"];
                        int endtxblockheight = -1;
                        int endtxblockindex = -1;
                        int vinputN = -1;
                        if (!string.IsNullOrEmpty(endtxid))
                        {
                            findFliter = "{txid:'" + (endtxid).formatHexStr() + "'}";
                            JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                            JOTx = (JObject)JATx[0];
                            endtxblockheight = (int)JOTx["blockindex"];
                            JArray JAvin = (JArray)JOTx["vin"];
                            findFliter = "{index:" + endtxblockheight + "}";
                            result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                            for (var i = 0; i < result.Count; i++)
                            {
                                JObject Jo = (JObject)result[i];
                                if (endtxid == (string)Jo["txid"])
                                {
                                    endtxblockindex = i;
                                }
                            }
                            for (var i = 0; i < JAvin.Count; i++)
                            {
                                JObject Jo = (JObject)JAvin[i];
                                if ((string)Jo["txid"] == starttxid && voutN == i)
                                {
                                    vinputN = i;
                                }
                            }

                        }
                        else
                        {
                        }

                        JOresult = new JObject();
                        JOresult["starttxid"] = starttxid;
                        JOresult["starttxblockheight"] = starttxblockheight;
                        JOresult["starttxblockindex"] = starttxblockindex;
                        JOresult["voutN"] = voutN;
                        JOresult["endtxid"] = endtxid;
                        JOresult["endtxblockheight"] = endtxblockheight;
                        JOresult["endtxblockindex"] = endtxblockindex;
                        JOresult["vinputN"] = vinputN;
                        result = new JArray() { JOresult };

                        break;
                    case "getTxByTxid":
                        txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + txid + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        break;
                    case "getTxDetail":
                        txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + txid + "'}";
                        result = bu.getTxDetail(mongodbConnStr, mongodbDatabase, txid);
                        break;
                    case "getTx":
                        if (req.@params.Count() == 3)
                        {
                            string type = (string)req.@params[0];
                            findFliter = "{type:'" + type + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getTransByAddresscount":
                        if (req.@params.Count() == 1)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                        }
                        else {
                            findFliter = "{}";
                        }
                        result = getJAbyKV("transcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address_tx", findFliter));
                        break;
                    case "getTransByAddress":
                        if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getOperatedFeeByAddrCount":
                        if (req.@params.Count() == 1)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                        }
                        else
                        {
                            findFliter = "{}";
                        }
                        result = getJAbyKV("operatedcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "operatedFee", findFliter));
                        break;
                    case "getOperatedFeeByAddr":
                        if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedFee", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedFee", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getOperatedFeeBySARCount":
                        if (req.@params.Count() == 1)
                        {
                            string sarTxid = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{sarTxid:'" + sarTxid + "'}";
                        }
                        else
                        {
                            findFliter = "{}";
                        }
                        result = getJAbyKV("operatedcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "operatedFee", findFliter));
                        break;
                    case "getOperatedFee":
                        address = string.Empty;
                        if (req.@params.Count() == 1)
                        {
                            address = (string)req.@params[0];
                            
                        }
                        result = getJAbyJ(bu.getOperatedFee(mongodbConnStr, mongodbDatabase, address));
                        break;
                    case "getOperatedFeeBySAR":
                        if (req.@params.Count() == 3)
                        {
                            string sarTxid = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{sarTxid:'" + sarTxid + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedFee", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedFee", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getStaticReport":
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        result = getJAbyJ(bu.getStaticReport(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSDUSD.formatHexStr(),hashSNEO.formatHexStr(),hashSAR4C.formatHexStr(),hashORACLE.formatHexStr(),addrSAR4C,oldAddrSAR4C));
                        break;
                    case "getAllassetBalance":
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        if (req.@params.Count() == 1)
                        {
                            address = (string)req.@params[0];
                            result = bu.getAllassetBalance(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, address, hashSAR4B.formatHexStr(), hashTokenized.formatHexStr(), hashORACLE.formatHexStr(),hashSDUSD.formatHexStr(),hashSNEO.formatHexStr(),hashSDS.formatHexStr());
                        }
                        break;
                    case "getOracleConfig":
                        if (req.@params.Count() == 2)
                        {
                            string type = (string)req.@params[0];
                            string key = (string)req.@params[1];
                            result = bu.getOracleConfig(neoCliJsonRPCUrl, hashORACLE.formatHexStr(), type, key);
                        }
                        else if (req.@params.Count() == 1)
                        {
                            string type = (string)req.@params[0];
                            result = bu.getOracleConfig(neoCliJsonRPCUrl, hashORACLE.formatHexStr(), type, "");
                        }
                        else {
                            result = bu.getOracleConfig(neoCliJsonRPCUrl, hashORACLE.formatHexStr(), "", "");
                        }
                        break;
                    case "getUsableUtxoForNEO":
                        if (req.@params.Count() == 1)
                        {
                            hashSNEO = ((string)req.@params[0]).formatHexStr();
                        }
                        result = bu.getUsableUtxoForNEO(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSNEO.formatHexStr());
                        break;
                    case "testLostData":
                        int mount = int.Parse((string)req.@params[0]);
                        result = bu.transferSNEOdata(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, mount);
                        break;
                    case "processSARFilterByRate":
                        decimal start = decimal.Parse(req.@params[0].ToString());
                        decimal end = decimal.Parse(req.@params[1].ToString());
                        if (neoCliJsonRPCUrlLocal.Length <= 0) {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        result = bu.processSARFilterByRate(mongodbConnStr,mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C.formatHexStr(),hashORACLE.formatHexStr(),start,end);
                        break;
                    case "predictFeeTotal":
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        result = getJAbyJ(bu.predictFeeTotal(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, hashSAR4C.formatHexStr(), hashORACLE.formatHexStr()));
                        break;
                        //根据地址获取所有锁仓账户信息
                    case "getLockListByAdd":
                        assetID = (string)req.@params[0];
                        addr = (string)req.@params[1];
                        result = bu.getLockListByAdd(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID.formatHexStr(), addr);
                        break;
                    case "getLockTypeByAdd":
                        assetID = (string)req.@params[0];
                        addr = (string)req.@params[1];
                        result = getJAbyJ(bu.getLockTypeByAdd(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID.formatHexStr(), addr));
                        break;
                    case "getLockHistory":
                        if (req.@params.Count() == 4)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{addr:'" + addr + "',asset:'"+assetID+"'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedLock", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedLock", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "statcDataProcess":
                        result = getJAbyJ(bu.statcDataProcess(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSDUSD.formatHexStr(), hashSNEO.formatHexStr(), hashSAR4C.formatHexStr(), hashORACLE.formatHexStr(), addrSAR4C, oldAddrSAR4C));
                        break;
                    case "getstaticDataCount":
                        findFliter = "{}";
                        result = getJAbyKV("datacount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "Staticdata", findFliter));
                        break;
                    case "getstaticData": 
                        findFliter = "{}"; 
                        sortStr = "{'now':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "Staticdata", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        break;
                    case "getHandOutRecord":
                        assetID = (string)req.@params[0];
                        result = getJAbyJ(bu.getHandOutRecord(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID.formatHexStr()));
                        break;
                    case "getHandOutRecordById":
                        string batchId = (string)req.@params[0];
                        findFliter = "{batchId:'"+ batchId+"'}";
                        sortStr = "{}";
                        result = mh.GetData(mongodbConnStr,mongodbDatabase, "BonusRecord", findFliter);
                        break;
                    //账户系统相关操作    
                    case "getAccAppList"://app列表
                        assetID = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{'asset':'" + assetID + "',type:1}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "accOperator", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        break;
                    case "getAccApproveAppList":
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        string username = (string)req.@params[2];
                        result = bu.getAccApproveAppList(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr, username);
                        break;
                    case "getAccOperatorHis"://账户操作历史
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        findFliter = "{'asset':'" + assetID + "','addr':'"+ addr + "'}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "accOperator", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        break;
                    case "getAccBalanceByAdd"://账户余额
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        username = (string)req.@params[2];
                        result = bu.getAccBalanceByAdd(mongodbConnStr, mongodbDatabase,neoCliJsonRPCUrl,assetID,addr,username);
                        break;
                    case "getAccApproveHis"://账户授权记录
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        findFliter = "{'asset':'" + assetID + "','from':'" + addr + "'}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "accApproveOperator", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        break;
                    case "getAccTransferHis"://账户转账历史
                        if (req.@params.Count() == 4)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{'asset':'" + assetID + "','from':'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "accUserTransfer", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 5)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            string to = (string)req.@params[2];
                            findFliter = "{'asset':'" + assetID + "','from':'" + addr +"','to':'"+ to +"'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "accUserTransfer", sortStr, int.Parse(req.@params[3].ToString()), int.Parse(req.@params[4].ToString()), findFliter);
                        }
                        break;
                    case "getAccTransferByTxid":
                        txid = (string)req.@params[0];
                        findFliter = "{'txid':'" + txid + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "accUserTransfer",findFliter);
                        break;
                    case "getAccAppDetail"://app详细信息
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        username = (string)req.@params[2];
                        result = getJAbyJ(bu.getAccAppDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr, username));
                        break;
                    case "getGoodsList"://goods列表
                        if (req.@params.Count() == 3)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{'asset':'" + assetID + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsInit", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 4)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{'asset':'" + assetID + "','from':'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsInit", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getGoodsBalance":
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        result = bu.getGoodsBalance(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, addr, assetID, true);
                        break;
                    case "getSignForAdd":
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        result = bu.getSignForAdd(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, assetID, addr,wif);
                        break;
                    case "getSignInfo":
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        DateTime dt = DateTime.Now;
                        dt.AddHours(-8);
                        string str = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                        findFliter = "{asset:'" + assetID + "',addr:'" + addr + "',now:{'$gte':ISODate('" + str + "T00:00:00.000Z'),'$lte':ISODate('" + str + "T23:59:59.000Z')}}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "goodsSign", findFliter);
                        break;
                    case "getSignByStatus":
                        if (req.@params.Count() == 3)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{asset:'" + assetID + "'}";
                            sortStr = "{'now':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsSign", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 4)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                            sortStr = "{'now':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsSign", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getSignCountByStatus":    //记录总条数
                        if (req.@params.Count() == 1)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            findFliter = "{asset:'" + assetID + "'}";

                        }
                        else if (req.@params.Count() == 2)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                        }
                        result = getJAbyKV("signcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "goodsSign", findFliter));
                        break;
                    case "setSignForAdd":
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        result = getJAbyJ(bu.setSignForAdd(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr));
                        break;
                    case "setRefundFlagByTxid":
                        JArray arrays = (JArray)req.@params[0];
                        addr = (string)req.@params[1];
                        result = getJAbyJ(bu.setRefundFlagByTxid(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, arrays,addr));
                        break;
                    case "setRefundErrorFlagByTxid":
                        arrays = (JArray)req.@params[0];
                        addr = (string)req.@params[1];
                        string msg = (string)req.@params[2];
                        result = getJAbyJ(bu.setRefundErrorFlagByTxid(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, arrays, addr,msg));
                        break;
                    case "setGuessResult"://设置猜测单双
                        assetID = ((string)req.@params[0]).formatHexStr();
                        addr = (string)req.@params[1];
                        int ret = int.Parse((string)req.@params[2]);
                        mount = int.Parse((string)req.@params[3]);
                        result = getJAbyJ(bu.setGuessResult(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID,addr, ret,mount,wif));
                        break;
                    case "getGuessByStatus"://查询记录
                        if (req.@params.Count() == 4)//查询地址下所有猜测记录
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                            sortStr = "{'now':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsGuess", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 5)//查询猜测结果为正确的记录
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            ret = int.Parse((string)req.@params[2]);
                            findFliter = "{addr:'" + addr +"',asset:'"+assetID+"',result:" + ret + "}";
                            sortStr = "{'now':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsGuess", sortStr, int.Parse(req.@params[3].ToString()), int.Parse(req.@params[4].ToString()), findFliter);
                        }
                        break;
                    case "getGuessCountByStatus":    //记录总条数
                        if (req.@params.Count() == 2)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                        }
                        else if (req.@params.Count() == 3)
                        {
                            assetID = ((string)req.@params[0]).formatHexStr();
                            addr = (string)req.@params[1];
                            ret = int.Parse((string)req.@params[2]);
                            findFliter = "{addr:'" + addr + "',asset:'" + assetID + "',result:" + ret + "}";
                        }
                        result = getJAbyKV("guesscount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "goodsGuess", findFliter));
                        break;
                    case "setGuessStatus"://设置发放记录
                        if (neoCliJsonRPCUrlLocal.Length <= 0)
                        {
                            neoCliJsonRPCUrlLocal = neoCliJsonRPCUrl;
                        }
                        assetID = ((string)req.@params[0]).formatHexStr();
                        result = getJAbyJ(bu.ProcessGoodsGuessInfo(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, assetID,wif));
                        break;
                    case "addAuctionGoods":   //增加拍卖物品
                        assetID = ((string)req.@params[0]).formatHexStr();  //goods合约
                        txid = ((string)req.@params[1]).formatHexStr();            //goods的txid
                        result = getJAbyJ(bu.addAuctionGoods(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrlLocal, assetID, txid));
                        break;
                    case "getAuctionByStatus"://查询拍卖物品
                        findFliter = "{}";
                        int pageCount = 10;
                        int pageNum = 1;
                        if (req.@params.Count() == 3)
                        {
                            int status = int.Parse((string)req.@params[0]);
                            pageCount = int.Parse(req.@params[1].ToString());
                            pageNum = int.Parse(req.@params[2].ToString());

                            findFliter = "{status:" + status + "}";
                            sortStr = "{'now':-1}";
                        } else if (req.@params.Count() == 2) {
                            pageCount = int.Parse(req.@params[0].ToString());
                            pageNum = int.Parse(req.@params[1].ToString());

                            sortStr = "{'now':-1}";
                        }
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "auctionGoods", sortStr, pageCount, pageNum, findFliter);
                        result = bu.getAuctionByStatus(mongodbConnStr, mongodbDatabase, result);
                        break;
                    case "getAuctionByStatusCount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            int status = int.Parse((string)req.@params[0]);
                            findFliter = "{status:" + status + "}";
                        }
                        result = getJAbyKV("auctioncount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "auctionGoods", findFliter));
                        break;
                    case "setAuctionResult"://设置拍卖结果
                        assetID = ((string)req.@params[0]).formatHexStr(); //account合约地址
                        addr = (string)req.@params[1];
                        string auctionKey = (string)req.@params[2];//拍卖物品key
                        mount = int.Parse((string)req.@params[3]);//拍卖出价金额
                        result = getJAbyJ(bu.setAuctionResult(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr, auctionKey, mount, wif));
                        break;
                    case "processAuctionResult"://处理拍卖逻辑
                        //assetID = ((string)req.@params[0]).formatHexStr();
                        result = getJAbyJ(bu.processAuctionResult(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, ""));
                        break;
                    case "getAuctionRecords"://查询拍卖记录
                        findFliter = "{}";
                        pageCount = 10;
                        pageNum = 1;
                        if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'now':-1}";
                            pageCount = int.Parse(req.@params[1].ToString());
                            pageNum = int.Parse(req.@params[2].ToString());
                        }
                        else if (req.@params.Count() == 2)
                        {
                            sortStr = "{'now':-1}";
                            pageCount = int.Parse(req.@params[0].ToString());
                            pageNum = int.Parse(req.@params[1].ToString());
                        }
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "auctionRecord", sortStr,pageCount, pageNum, findFliter);
                        break;
                    case "getAuctionRecordsCount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                        }
                        result = getJAbyKV("recordcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "auctionRecord", findFliter));
                        break;
                    case "getAuctionRecordsByName"://查询拍卖记录
                        findFliter = "{}";
                        pageCount = 10;
                        pageNum = 1;
                        if (req.@params.Count() == 3)
                        {
                            string key = (string)req.@params[0];
                            pageCount = int.Parse(req.@params[1].ToString());
                            pageNum = int.Parse(req.@params[2].ToString());

                            findFliter = "{key:'" + key + "'}";
                            sortStr = "{'now':-1}";
                        }
                        else if (req.@params.Count() == 4)
                        {
                            addr = (string)req.@params[0];
                            string key = (string)req.@params[1];
                            pageCount = int.Parse(req.@params[2].ToString());
                            pageNum = int.Parse(req.@params[3].ToString());

                            findFliter = "{key:'" + key +",addr:'"+addr+"'}";
                            sortStr = "{'now':-1}";
                        }
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "auctionRecord", sortStr, pageCount, pageNum, findFliter);
                        break;
                    case "getAuctionRecordsByNameCount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            string key = (string)req.@params[0];
                            findFliter = "{key:'" + key + "'}";
                        }
                        else if (req.@params.Count() == 2)
                        {
                            addr = (string)req.@params[0];
                            string key = (string)req.@params[1];
                            findFliter = "{key:'" + key + ",addr:'" + addr + "'}";
                        }
                        result = getJAbyKV("recordcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "auctionRecord", findFliter));
                        break;

                }
                if (result != null && result.Count > 0 && result[0]["errorCode"] != null)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, (int)result[0]["errorCode"], (string)result[0]["errorMsg"], (string)result[0]["errorData"]);
                    return resE;    
                }
                if (result.Count == 0)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -1, "No Data", "Data does not exist");

                    return resE;
                }
            }
            catch (Exception e)
            {
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -100, "Parameter Error", e.Message);

                return resE;

            }

            JsonPRCresponse res = new JsonPRCresponse();
            res.jsonrpc = req.jsonrpc;
            res.id = req.id;
            res.result = result;

            return res;
        }
    }
}

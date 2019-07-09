using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using ThinNeo;
using NEO_Block_API.lib;

namespace Block_API.lib
{
    public class Comm
    {
        public static string api_SendbatchTransaction(byte[] prikey, Hash160 schash, string methodname, params string[] subparam)
        {
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                byte[] randombytes = new byte[32];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randombytes);
                }
                BigInteger randomNum = new BigInteger(randombytes);
                sb.EmitPushNumber(randomNum);
                sb.Emit(ThinNeo.VM.OpCode.DROP);

                if (subparam != null && subparam.Length > 0)
                {
                    for (var i = 0; i < subparam.Length; i++)
                    {
                        array.AddArrayValue(subparam[i]);
                    }
                }
                sb.EmitParamJson(array);
                sb.EmitPushString(methodname);
                sb.EmitAppCall(schash);
                data = sb.ToArray();
            }
            //MakeTran
            ThinNeo.Transaction tran = new ThinNeo.Transaction();
            tran.version = 0;//0 or 1
            tran.inputs = new ThinNeo.TransactionInput[0];
            tran.outputs = new ThinNeo.TransactionOutput[0];
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            tran.extdata = new ThinNeo.InvokeTransData();
            var idata = new ThinNeo.InvokeTransData();
            tran.extdata = idata;
            idata.script = data;
            idata.gas = 0;

            tran.attributes = new ThinNeo.Attribute[1];
            tran.attributes[0] = new ThinNeo.Attribute();
            tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
            tran.attributes[0].data = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            return ThinNeo.Helper.Bytes2HexString(trandata);
        }

    }

}
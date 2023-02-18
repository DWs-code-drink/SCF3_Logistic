using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SCF3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            ConsoleKeyInfo cki;
            string s = AppDomain.CurrentDomain.BaseDirectory;
            StringBuilder sb = new StringBuilder(s);
            if(!s.EndsWith("\\"))
            {
                sb.Append("\\");
                s = sb.ToString();
            }
            sb.Append("Results\\");
            DirectoryInfo di = new DirectoryInfo(sb.ToString());
            if (!Directory.Exists(di.FullName))
            {
                Directory.CreateDirectory(di.FullName);
            }
            while (true)
            {
                Console.WriteLine("请选择任务/ジョブを選んでください/Please choose a job");
                Console.WriteLine("[R] x/y/ 2D");
                Console.WriteLine("[E] x/y/z 3D");
                Console.WriteLine("[M] 0/1/x 3D");
                Console.WriteLine("[I] 0/x/(x^2) 3D");
                Console.WriteLine("[F] 0/x/ln(x) 3D");
                Console.WriteLine("[L] 0/x/exp(x) 3D");
                Console.WriteLine("[A] 0/x/sigmoid(x) 3D");
                Console.WriteLine("[N] 0/x/(1/x) 3D");
                Console.WriteLine("[4] 7/29/xy 3D");
                Console.WriteLine("[9] 7/29/(exp(centralized x)) 3D");
                Console.WriteLine("[5] 7/29/((centralized x)^2) 3D");
                cki = Console.ReadKey();
                Console.WriteLine("Please wait.");
                switch(cki.Key)
                {
                    case ConsoleKey.R:
                        {
                            SKSCF3.SCF3sgm();
                            break;
                        }
                    case ConsoleKey.E:
                        {
                            SKSCF3.SCF33sgm();
                            break;
                        }
                    case ConsoleKey.M:
                        {
                            SKSCF3.SCF3csgm2D();
                            break;
                        }
                    case ConsoleKey.I:
                        {
                            SKSCF3.SCF3csgm2D2();
                            break;
                        }
                    case ConsoleKey.F:
                        {
                            SKSCF3.SCF3csgmln();
                            break;
                        }
                    case ConsoleKey.L:
                        {
                            SKSCF3.SCF3csgmexp();
                            break;
                        }
                    case ConsoleKey.A:
                        {
                            SKSCF3.SCF3csgmsgm();
                            break;
                        }
                    case ConsoleKey.N:
                        {
                            SKSCF3.SCF3csgmr();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            SKSCF3.SCF3csgmkz();
                            break;
                        }
                    case ConsoleKey.D9:
                        {
                            SKSCF3.SCF3csgmCE();
                            break;
                        }
                    case ConsoleKey.D5:
                        {
                            SKSCF3.SCF3csgmCS();
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Incorrect key.");
                            break;
                        }
                }
            }
        }
    }
    internal class Rgrsgm
    {
        private readonly double[][] des;
        private readonly double[] y;
        private readonly double yvar;
        private double[] w;
        private double[] z;
        private double[] s;
        private double[] ds;
        private object[] lods;
        private double[] E;
        private double Es;
        private double sf;
        private double py;
        private double R21;
        private ParallelOptions po;
        private CancellationTokenSource cts;
        internal double zwc
        {
            get
            {
                return Es;
            }
        }
        internal double _R21
        {
            get
            {
                return R21;
            }
        }
        internal double xs(int i)
        {
            return w[i];
        }
        internal Rgrsgm(in double[] Y, in double[][] D, in double sk, in double hi, in double yv)
        {
            if (D.Length <= 1)
            {
                Console.WriteLine("Invalid length.");
                return;
            }
            if (sk == 0.0)
            {
                Console.WriteLine("Invalid scale.");
                return;
            }
            if (D[0].Length != Y.Length)
            {
                Console.WriteLine("Can not verify data amount.");
                return;
            }
            if (yv <= 0.0)
            {
                Console.WriteLine("Invalid data variance.");
                return;
            }
            int cnt, cnt2;
            des = D;
            sf = sk;
            py = hi;
            y = Y;
            yvar = yv;
            for (cnt = 0; cnt < D.Length; cnt++)
            {
                if (D[cnt].Length != y.Length)
                {
                    Console.WriteLine("Invalid length.");
                    return;
                }
            }
            cts = new CancellationTokenSource();
            po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;
            po.TaskScheduler = TaskScheduler.Default;
            z = new double[des[0].Length];
            s = new double[des[0].Length];
            w = new double[des.Length];
            ds = new double[des.Length];
            lods = new object[des.Length];
            E = new double[des[0].Length];
            Es = 0.0;
            Random rnd = new Random();
            double dtemp;
            for (cnt = 0; cnt < w.Length; cnt++)
            {
                dtemp = 0.0;
                for (cnt2 = 0; cnt2 < des[cnt].Length; cnt2++)
                {
                    dtemp += des[cnt][cnt2];
                }
                if (dtemp != 0.0) dtemp = des[cnt].Length / dtemp;
                else dtemp = 0.3434;
                w[cnt] = rnd.NextDouble() * dtemp - dtemp / 2.0;
                lods[cnt] = new object();
            }
        }
        internal bool ks()
        {
            int cnt;
            for (cnt = 0; cnt < ds.Length; cnt++)
            {
                ds[cnt] = 0.0;
            }
            Es = 0.0;
            Parallel.For(0, des[0].Length, po, (dind) =>
            {
                int cntp;
                double dtempp = 0, dtempp2;
                for (cntp = 0; cntp < des.Length; cntp++)
                {
                    dtempp += des[cntp][dind] * w[cntp];
                }
                z[dind] = dtempp;
                s[dind] = sf / (1.0 + Math.Exp(-z[dind])) + py;
                E[dind] = s[dind] - y[dind];
                lock (E.SyncRoot) Es += Math.Abs(E[dind]);
                dtempp = E[dind] * s[dind] * (1.0 - s[dind]);
                for (cntp = 0; cntp < des.Length; cntp++)
                {
                    dtempp2 = dtempp * des[cntp][dind];
                    lock (lods[cntp]) ds[cntp] += dtempp2;
                }
            });
            return true;
        }
        internal bool stk(in int ep)
        {
            double Esp, lr = 1.04;
            int cnt, cnt2;
            bool btemp;
            for (cnt = 0; cnt < ep; cnt++)
            {
                btemp = ks();
                if (!btemp)
                {
                    Console.WriteLine("Can not calculate.");
                    return false;
                }
                if (Es <= 0.003434) break;
                for (cnt2 = 0; cnt2 < w.Length; cnt2++)
                {
                    if (Math.Abs(ds[cnt2] * 100.0) < Math.Abs(w[cnt2]))
                    {
                        w[cnt2] -= Math.CopySign(w[cnt2] / 100.0, ds[cnt2]);
                    }
                    else w[cnt2] -= lr * ds[cnt2];
                }
            }
            R21 = 0.0;
            for (cnt = 0; cnt < y.Length; cnt++)
            {
                Esp = y[cnt] - s[cnt];
                R21 += Esp * Esp;
            }
            R21 = 1 - (R21 / y.Length / yvar);
            return true;
        }
    }
    static internal class DatRW
    {
        static private Regex rcsv = new Regex(@"^(?:(?>([^,]+)),?)+$", RegexOptions.Compiled, TimeSpan.FromSeconds(3.434));
        static internal bool ResR(in string fpth, out double[] yr, out double var)
        {
            yr = null;
            var = double.NaN;
            if (!File.Exists(fpth))
            {
                Console.WriteLine("Cna not find result file.");
                return false;
            }
            string stemp;
            double dtemp;
            bool flg;
            List<double> dl = new List<double>();
            using (FileStream fs = new FileStream(fpth, FileMode.Open, FileAccess.Read, FileShare.None, 128))
            {
                using (StreamReader sr = new StreamReader(fs, true))
                {
                    do
                    {
                        stemp = sr.ReadLine();
                        flg = double.TryParse((stemp ?? string.Empty).Trim(), out dtemp);
                        if (!flg)
                        {
                            yr = dl.ToArray();
                            break;
                        }
                        else dl.Add(dtemp);
                    }
                    while (stemp != null);
                }
            }
            if (yr == null)
            {
                Console.WriteLine("Invalid result data.");
                return false;
            }
            else if (yr.Length <= 1)
            {
                Console.WriteLine("Invalid result data amount.");
                return false;
            }
            else
            {
                dtemp = 0.0;
                double atemp;
                var = 0.0;
                for (int cnt = 0; cnt < yr.Length; cnt++)
                {
                    dtemp += yr[cnt];
                }
                atemp = dtemp / yr.Length;
                for (int cnt = 0; cnt < yr.Length; cnt++)
                {
                    dtemp = yr[cnt] - atemp;
                    var += dtemp * dtemp;
                }
                var /= yr.Length;
            }
            return true;
        }
        static internal bool DesR(in string fpth, out double[][] desr)
        {
            desr = null;
            if (!File.Exists(fpth))
            {
                Console.WriteLine("Cna not find descriptor file.");
                return false;
            }
            string stemp;
            double dtemp;
            bool flg;
            List<double>[] destemp;
            Match m;
            int cnt, ivf = 0, ivf2 = 0;
            using (FileStream fs = new FileStream(fpth, FileMode.Open, FileAccess.Read, FileShare.None, 128))
            {
                using (StreamReader sr = new StreamReader(fs, true))
                {
                    stemp = sr.ReadLine();
                    m = rcsv.Match(stemp ?? string.Empty);
                    if (!m.Success)
                    {
                        Console.WriteLine("Invalid descriptor information.");
                        return false;
                    }
                    destemp = new List<double>[m.Groups[1].Captures.Count];
                    for (cnt = 0; cnt < m.Groups[1].Captures.Count;)
                    {
                        flg = double.TryParse(m.Groups[1].Captures[cnt].Value.Trim(), out dtemp);
                        if (!flg)
                        {
                            ivf2 = cnt;
                            Console.WriteLine("Warning : Can not verify descriptor amount, {0} read.", ivf2);
                            break;
                        }
                        destemp[cnt] = new List<double>();
                        destemp[cnt].Add(dtemp);
                        cnt++;
                        if (cnt == m.Groups[1].Captures.Count)
                        {
                            ivf2 = cnt;
                        }
                    }
                    if (ivf2 < 1)
                    {
                        Console.WriteLine("Invalid descriptors.");
                    }
                    else
                    {
                        ivf++;
                        do
                        {
                            stemp = sr.ReadLine();
                            m = rcsv.Match(stemp ?? string.Empty);
                            if (!m.Success)
                            {
                                break;
                            }
                            if (m.Groups[1].Captures.Count != ivf2)
                            {
                                Console.WriteLine("Can not verify descriptor amount, line {0}, {1} read.", ivf + 1, m.Groups[1].Captures.Count);
                            }
                            for (cnt = 0; cnt < ivf2; cnt++)
                            {
                                flg = double.TryParse(m.Groups[1].Captures[cnt].Value.Trim(), out dtemp);
                                if (!flg)
                                {
                                    Console.WriteLine("Invalid line ({0}), {1} read.", ivf + 1, cnt + 1);
                                    return false;
                                }
                                destemp[cnt].Add(dtemp);
                            }
                            ivf++;
                        }
                        while (stemp != null);
                    }
                }
            }
            desr = new double[ivf2][];
            for (cnt = 0; cnt < desr.Length; cnt++)
            {
                desr[cnt] = destemp[cnt].ToArray();
                if (desr[cnt].Length != ivf)
                {
                    Console.WriteLine("Can not verify data amount of descriptor {0}.", cnt + 1);
                    return false;
                }
            }
            return true;
        }
    }
    static internal class SKSCF3
    {
        static private readonly string respth = AppDomain.CurrentDomain.BaseDirectory+"\\Data\\SCF3res.csv";
        static private readonly string despth = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\SCF3des.csv";
        static private readonly string despth2 = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\SCF3des2.csv";
        static private readonly string despth3 = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\SCF3des3.csv";
        static private readonly string sgmrpth = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGM\\";
        static private readonly string sgmr3pth = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\3SGM\\";
        static private readonly string sgmr3cpth = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\3SGMC\\";
        static private readonly string sgmrcpth2D = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGM2D\\";
        static private readonly string sgmrcpth2D2 = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGM2D2\\";
        static private readonly string sgmrcpthln = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMln\\";
        static private readonly string sgmrcpthexp = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMexp\\";
        static private readonly string sgmrcpthsgm = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMsgm\\";
        static private readonly string sgmrcpthr = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMr\\";
        static private readonly string sgmrcpthkz = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMkz\\";
        static private readonly string sgmrcpthCE = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMCE\\";
        static private readonly string sgmrcpthCS = AppDomain.CurrentDomain.BaseDirectory + "\\Results\\SGMCS\\";
        static private readonly double sk = 1.0;
        static private readonly double hi = 0.0;
        static private readonly int ep = 34340;
        static internal void SCF3sgm()
        {
            if(!Directory.Exists(sgmrpth))
            {
                Directory.CreateDirectory(sgmrpth);
            }
            double[] y;
            bool flg;
            int totr, cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrpth);
            Rgrsgm sgmr;
            List<Tuple<int[], double>> Rest = new List<Tuple<int[], double>>();
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            totr = des.Length * (des.Length - 1) / 2;
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            for (cnt = 0; cnt < des.Length; cnt++)
            {
                for (cnt2 = 0; cnt2 < cnt;)
                {
                    desin = new double[3][];
                    desin[0] = des[cnt];
                    desin[1] = des[cnt2];
                    desin[2] = bias;
                    sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                    flg = sgmr.stk(ep);
                    if (!flg)
                    {
                        Console.WriteLine("Error ({0},{1}) : Regression failed.", cnt, cnt2);
                    }
                    Rest.Add(new Tuple<int[], double>(new int[2] { cnt, cnt2 }, sgmr.zwc));
                    sb.Append(sgmr.zwc.ToString("G15"));
                    sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2)));
                    sb3.Append(sgmr._R21.ToString("G15"));
                    cnt2++;
                    if (cnt2 != cnt)
                    {
                        sb.Append(", ");
                        sb2.Append(", ");
                        sb3.Append(", ");
                    }
                    else break;
                }
                for (cnt2 = cnt; cnt2 < des.Length; cnt2++)
                {
                    if (cnt2 == des.GetUpperBound(0))
                    {
                        sb.Append("NA");
                        sb2.Append("NA");
                        sb3.Append("NA");
                        break;
                    }
                    if (cnt2 == cnt && cnt != 0)
                    {
                        sb.Append(", ");
                        sb2.Append(", ");
                        sb3.Append(", ");
                    }
                    sb.Append("NA, ");
                    sb2.Append("NA, ");
                    sb3.Append("NA, ");
                }
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrpth);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrpth);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
        static internal void SCF33sgm()
        {
            if (!Directory.Exists(sgmr3pth))
            {
                Directory.CreateDirectory(sgmr3pth);
            }
            double[] y;
            bool flg;
            int cnt, cnt2, cnt3;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmr3pth);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            int[] des3_1 = new int[9] { 1, 7, 11, 12, 21, 23, 24, 29, 30 };
            for (cnt = 1; cnt < des3_1.Length; cnt++)
            {
                for (cnt2 = 0; cnt2 < cnt; cnt2++)
                {
                    sb5.AppendLine(string.Format("{0},{1}", cnt, cnt2));
                    for (cnt3 = 0; cnt3 < des.Length; cnt3++)
                    {
                        if (cnt3 == des3_1[cnt] || cnt3 == des3_1[cnt2]) continue;
                        desin = new double[4][];
                        desin[0] = des[des3_1[cnt]];
                        desin[1] = des[des3_1[cnt2]];
                        desin[2] = des[cnt3];
                        desin[3] = bias;
                        sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                        flg = sgmr.stk(ep);
                        if (!flg)
                        {
                            Console.WriteLine("Error ({0},{1},{2}) : Regression failed.", cnt, cnt2, cnt3);
                        }
                        sb.Append(sgmr.zwc.ToString("G15"));
                        sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                        sb3.Append(sgmr._R21.ToString("G15"));
                        cnt3++;
                        if (cnt3 != des.GetUpperBound(0))
                        {
                            sb.Append(", ");
                            sb2.Append(", ");
                            sb3.Append(", ");
                        }
                    }
                    if (!((cnt == des3_1.GetUpperBound(0)) && (cnt2 == cnt - 1)))
                    {
                        sb.AppendLine("");
                        sb2.AppendLine("");
                        sb3.AppendLine("");
                    }
                    Console.WriteLine("{0},{1}", cnt, cnt2);
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3pth);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3pth);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3pth);
            sb4.Append("comb.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF33csgm()
        {
            if (!Directory.Exists(sgmr3cpth))
            {
                Directory.CreateDirectory(sgmr3cpth);
            }
            double[] y;
            bool flg;
            int cnt, cnt2, cnt3;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmr3cpth);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            for (cnt = 2; cnt < des.Length;)
            {
                for (cnt2 = 0; cnt2 < cnt;)
                {
                    for (cnt3 = 0; cnt3 < cnt2;)
                    {
                        desin = new double[4][];
                        desin[0] = des[cnt];
                        desin[1] = des[cnt2];
                        desin[2] = des[cnt3];
                        desin[3] = bias;
                        sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                        flg = sgmr.stk(ep);
                        if (!flg)
                        {
                            Console.WriteLine("Error ({0},{1},{2}) : Regression failed.", cnt, cnt2, cnt3);
                        }
                        sb.Append(sgmr.zwc.ToString("G15"));
                        sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                        sb3.Append(sgmr._R21.ToString("G15"));
                        sb5.Append(string.Format("({0}-{1}-{2})", cnt, cnt2, cnt3));
                        cnt3++;
                        if (cnt3 != cnt2)
                        {
                            sb.Append(", ");
                            sb2.Append(", ");
                            sb3.Append(", ");
                            sb5.Append(", ");
                        }
                    }
                    cnt2++;
                    if (cnt2 != cnt)
                    {
                        sb.AppendLine("");
                        sb2.AppendLine("");
                        sb3.AppendLine("");
                        sb5.AppendLine("");
                    }
                }
                Console.WriteLine(cnt);
                cnt++;
                if (cnt != des.Length)
                {
                    sb.AppendLine("");
                    sb2.AppendLine("");
                    sb3.AppendLine("");
                    sb5.AppendLine("");
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3cpth);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3cpth);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmr3cpth);
            sb4.Append("comb.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF3csgm2D()
        {
            if (!Directory.Exists(sgmrcpth2D))
            {
                Directory.CreateDirectory(sgmrcpth2D);
            }
            double[] y;
            bool flg;
            int cnt;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpth2D);
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth2, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[1];
                desin[2] = des[cnt];
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,1,{2}) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpth2D);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpth2D);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
        static internal void SCF3csgm2D2()
        {
            if (!Directory.Exists(sgmrcpth2D2))
            {
                Directory.CreateDirectory(sgmrcpth2D2);
            }
            double[] y;
            bool flg;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpth2D2);
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] des2d;
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                des2d = new double[des[0].Length];
                for (cnt2 = 0; cnt2 < des2d.Length; cnt2++)
                {
                    des2d[cnt2] = des[cnt][cnt2] * des[cnt][cnt2];
                }
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[cnt];
                desin[2] = des2d;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}s) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpth2D2);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpth2D2);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
        static internal void SCF3csgmln()
        {
            if (!Directory.Exists(sgmrcpthln))
            {
                Directory.CreateDirectory(sgmrcpthln);
            }
            double[] y;
            bool flg, flg2;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthln);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth2, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] des2d;
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                flg2 = true;
                des2d = new double[des[0].Length];
                for (cnt2 = 0; cnt2 < des2d.Length; cnt2++)
                {
                    if (des[cnt][cnt2] <= 0.0)
                    {
                        flg2 = false;
                        break;
                    }
                    des2d[cnt2] = Math.Log(des[cnt][cnt2]);
                }
                if (!flg2) continue;
                sb5.AppendLine(cnt.ToString("G"));
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[cnt];
                desin[2] = des2d;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthln);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthln);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthln);
            sb4.Append("ind.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF3csgmexp()
        {
            if (!Directory.Exists(sgmrcpthexp))
            {
                Directory.CreateDirectory(sgmrcpthexp);
            }
            double[] y;
            bool flg, flg2;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthexp);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth2, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] des2d;
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                flg2 = true;
                des2d = new double[des[0].Length];
                for (cnt2 = 0; cnt2 < des2d.Length; cnt2++)
                {
                    des2d[cnt2] = Math.Exp(des[cnt][cnt2]);
                    if (double.IsInfinity(des2d[cnt2]) || double.IsNaN(des2d[cnt2]))
                    {
                        flg2 = false;
                        break;
                    }
                }
                if (!flg2) continue;
                sb5.AppendLine(cnt.ToString("G"));
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[cnt];
                desin[2] = des2d;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthexp);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthexp);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthexp);
            sb4.Append("ind.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF3csgmsgm()
        {
            if (!Directory.Exists(sgmrcpthsgm))
            {
                Directory.CreateDirectory(sgmrcpthsgm);
            }
            double[] y;
            bool flg;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthsgm);
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth2, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] des2d;
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                des2d = new double[des[0].Length];
                for (cnt2 = 0; cnt2 < des2d.Length; cnt2++)
                {
                    des2d[cnt2] = 1 / (1 + Math.Exp(-des[cnt][cnt2]));
                }
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[cnt];
                desin[2] = des2d;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthsgm);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthsgm);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
        static internal void SCF3csgmr()
        {
            if (!Directory.Exists(sgmrcpthr))
            {
                Directory.CreateDirectory(sgmrcpthr);
            }
            double[] y;
            bool flg, flg2;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthr);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth2, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] des2d;
            for (cnt = 2; cnt < des.Length; cnt++)
            {
                flg2 = true;
                des2d = new double[des[0].Length];
                for (cnt2 = 0; cnt2 < des2d.Length; cnt2++)
                {
                    des2d[cnt2] = 1.0 / des[cnt][cnt2];
                    if (double.IsInfinity(des2d[cnt2]) || double.IsNaN(des2d[cnt2]))
                    {
                        flg2 = false;
                        break;
                    }
                }
                if (!flg2) continue;
                sb5.AppendLine(cnt.ToString("G"));
                desin = new double[4][];
                desin[0] = des[0];
                desin[1] = des[cnt];
                desin[2] = des2d;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthr);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthr);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthr);
            sb4.Append("ind.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF3csgmkz()
        {
            if (!Directory.Exists(sgmrcpthkz))
            {
                Directory.CreateDirectory(sgmrcpthkz);
            }
            double[] y;
            bool flg;
            int cnt, cnt2, cnt3;
            double[] bias;
            double[][] desin;
            double yvar;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthkz);
            StringBuilder sb5 = new StringBuilder();
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] desm;
            for (cnt = 0; cnt < des.Length; cnt++)
            {
                for (cnt2 = 0; cnt2 <= cnt; cnt2++)
                {
                    desm = new double[des[0].Length];
                    for (cnt3 = 0; cnt3 < desm.Length; cnt3++)
                    {
                        desm[cnt3] = des[cnt][cnt3] * des[cnt2][cnt3];
                    }
                    desin = new double[4][];
                    desin[0] = des[7];
                    desin[1] = des[29];
                    desin[2] = desm;
                    desin[3] = bias;
                    sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                    flg = sgmr.stk(ep);
                    if (!flg)
                    {
                        Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                    }
                    sb.Append(sgmr.zwc.ToString("G15"));
                    sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                    sb3.Append(sgmr._R21.ToString("G15"));
                    sb5.Append(string.Format("({0:G},{1:G})", cnt, cnt2));
                    if (cnt2 != cnt)
                    {
                        sb.Append(", ");
                        sb2.Append(", ");
                        sb3.Append(", ");
                        sb5.Append(", ");
                    }
                }
                if (cnt != des.GetUpperBound(0))
                {
                    sb.AppendLine();
                    sb2.AppendLine();
                    sb3.AppendLine();
                    sb5.AppendLine();
                }
                Console.WriteLine(cnt);
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthkz);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthkz);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthkz);
            sb4.Append("ind.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb5.ToString());
                }
            }
        }
        static internal void SCF3csgmCE()
        {
            if (!Directory.Exists(sgmrcpthCE))
            {
                Directory.CreateDirectory(sgmrcpthCE);
            }
            double[] y;
            bool flg;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar, dtemp;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthCE);
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] desm;
            for (cnt = 0; cnt < des.Length; cnt++)
            {
                desm = new double[des[0].Length];
                dtemp = 0.0;
                for (cnt2 = 0; cnt2 < des[cnt].Length; cnt2++)
                {
                    dtemp += des[cnt][cnt2];
                }
                dtemp /= des[cnt].Length;
                for (cnt2 = 0; cnt2 < des[cnt].Length; cnt2++)
                {
                    desm[cnt2] = Math.Exp(des[cnt][cnt2] - dtemp);
                }
                desin = new double[4][];
                desin[0] = des[7];
                desin[1] = des[29];
                desin[2] = desm;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.Append(", ");
                    sb2.Append(", ");
                    sb3.Append(", ");
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthCE);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthCE);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
        static internal void SCF3csgmCS()
        {
            if (!Directory.Exists(sgmrcpthCS))
            {
                Directory.CreateDirectory(sgmrcpthCS);
            }
            double[] y;
            bool flg;
            int cnt, cnt2;
            double[] bias;
            double[][] desin;
            double yvar, dtemp;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder(sgmrcpthCS);
            Rgrsgm sgmr;
            flg = DatRW.ResR(in respth, out y, out yvar);
            if (yvar <= 0.0 || y.Length <= 1)
            {
                Console.WriteLine("Invalid result data.");
                return;
            }
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            double[][] des;
            flg = DatRW.DesR(in despth, out des);
            if (!flg)
            {
                Console.WriteLine("Error : Regression failed.");
                return;
            }
            if (des.Length < 2 || des[0].Length != y.Length || y.Length <= 1)
            {
                Console.WriteLine("Error : Invalid data, regression failed.");
                return;
            }
            bias = new double[y.Length];
            for (cnt = 0; cnt < bias.Length; cnt++)
            {
                bias[cnt] = 1.0;
            }
            double[] desm;
            for (cnt = 0; cnt < des.Length; cnt++)
            {
                desm = new double[des[0].Length];
                dtemp = 0.0;
                for (cnt2 = 0; cnt2 < des[cnt].Length; cnt2++)
                {
                    dtemp += des[cnt][cnt2];
                }
                dtemp /= des[cnt].Length;
                for (cnt2 = 0; cnt2 < des[cnt].Length; cnt2++)
                {
                    desm[cnt2] = (des[cnt][cnt2] - dtemp) * (des[cnt][cnt2] - dtemp);
                }
                desin = new double[4][];
                desin[0] = des[7];
                desin[1] = des[29];
                desin[2] = desm;
                desin[3] = bias;
                sgmr = new Rgrsgm(in y, in desin, in sk, in hi, in yvar);
                flg = sgmr.stk(ep);
                if (!flg)
                {
                    Console.WriteLine("Error (0,{2},{2}ln) : Regression failed.", cnt);
                }
                sb.Append(sgmr.zwc.ToString("G15"));
                sb2.Append(string.Format("({0:G15})({1:G15})({2:G15})({3:G15})", sgmr.xs(0), sgmr.xs(1), sgmr.xs(2), sgmr.xs(3)));
                sb3.Append(sgmr._R21.ToString("G15"));
                if (cnt != des.GetUpperBound(0))
                {
                    sb.Append(", ");
                    sb2.Append(", ");
                    sb3.Append(", ");
                }
            }
            sb4.Append("E.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthCS);
            sb4.Append("β.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb2.ToString());
                }
            }
            sb4 = new StringBuilder(sgmrcpthCS);
            sb4.Append("R21.csv");
            using (FileStream fs = new FileStream(sb4.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 128))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode, 128, false))
                {
                    sw.Write(sb3.ToString());
                }
            }
        }
    }
}
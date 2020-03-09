using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using Sap.Data.Hana;
using System.Data.Odbc;

namespace WpfSAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sqlCon = "Server=192.168.20.4;UID=sa;PASSWORD=DBADMIN;Database=SAP;Max Pool Size=400;Connect Timeout=600;";
        string smlCon = "Server=192.168.20.4;UID=sa;PASSWORD=DBADMIN;Database=Smilingfish;Max Pool Size=400;Connect Timeout=600;";

        //string sapCon = "DRIVER={HDBODBC};UID=SYSTEM;PWD=ISSkps1992;SERVERNODE=192.168.20.1:30015;DATABASE=SBOKPS_LIVE";
        //string odbcCon = "DRIVER={HDBODBC32};UID=SYSTEM;PWD=ISSkps1992;SERVERNODE=192.168.20.1:30015;DATABASE =SBOKPS_LIVE;";
        string sapCon = "Server=192.168.20.1:30015;UserID=SYSTEM;Password=ISSkps1992;";
        public MainWindow()
        {
            InitializeComponent();
        }

        private SqlConnection sqlConn()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = sqlCon;
                conn.Open();
                return conn;
            }            
        }

        

        private HanaConnection sapConn()
        {
            //using (SqlConnection conn = new SqlConnection())
            //{
            //    conn.ConnectionString = sapCon;
            //    conn.Open();
            //    return conn;
            //}
            HanaConnection conn = new HanaConnection(sapCon);
            try
            {
                conn.Open();
                               
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " +
                     ex.Errors[0].Message + " (" +
                     ex.Errors[0].NativeError.ToString() + ")",
                     "Failed to connect");
               
            }

            return conn;
        }

        private void btLotClict(object sender, RoutedEventArgs e)
        {
            exeSql("truncate table OIBT");

            HanaDataAdapter dta = new HanaDataAdapter();
            DataTable dt = new DataTable();
            string lSql = "SELECT T0.\"ItemCode\" ,T0.\"WhsCode\", T0.\"BatchNum\" ,T0.\"ItemName\" ,sum(T0.\"Quantity\") \"Quantity\" " +
                           "FROM \"SBOKPS_LIVE\".\"OIBT\" T0 " +
                           "where T0.\"WhsCode\" in ('A01', 'A02', 'A03', 'A04', 'A05', 'A06', 'B01', 'B02', 'B03', 'B04', 'B05', 'B06', 'B07', 'B08', 'B09', 'B10', 'B11', 'B12', 'B13', 'B14', 'B15', 'B16', 'B17', 'B18', 'B19', 'B20', 'B21', 'B22', 'B23', 'B24', 'C01', 'C02', 'C03', 'C04') " +
                           " and T0.\"Quantity\" > 0 " +
                           " group by T0.\"ItemCode\" ,T0.\"WhsCode\", T0.\"BatchNum\" ,T0.\"ItemName\" ";// +
                           //" order by T0.\"WhsCode\", T0.\"ItemCode\" ,T0.\"BatchNum \" ";

            dta = new HanaDataAdapter(lSql, sapConn());
            dta.Fill(dt);

            for (int i=0;i < dt.Rows.Count ; i++)
            {
                //insert SQL.SAP.OIBT
                insertOIBT(dt.Rows[i]["ItemCode"].ToString() , dt.Rows[i]["BatchNum"].ToString(), dt.Rows[i]["WhsCode"].ToString(), double .Parse ( dt.Rows[i]["Quantity"].ToString()));
                
            }

            MessageBox.Show("Ok");
        }
               
        private void insertOIBT(string ItemCode,string BatchNum, string WhsCode, double Quantity)
        {
           string lSql = "insert into OIBT ([ItemCode],[BatchNum],[WhsCode],[obQty])" +
                   "values ('" + ItemCode + "','" + BatchNum + "','" + WhsCode + "'," + Quantity.ToString() + ");";
            exeSql(lSql);
        }

        private void insertTBITRAN(string itnDocNo,DateTime itnDate ,int itnLine, string itnWht, string itnWhf, string itnDesc, string itnGoods, double itnQty, string itnUm, double itnStockQty, string itnStockUm)
        {
            //itnUM = 1=`กป`,2=แพ๊ก,3=ลัง
            string iSymbol = "1";
            if( itnQty < 0)
            {
                iSymbol = "2";
            }
            string lSql = "INSERT INTO [dbo].[TB_ITRAN] " +
                           " ([ITN_STS],[ITN_IDOCTYPE],[ITN_DOCNO],[ITN_DATE] " +
                           " ,[ITN_LINE],[ITN_DOCTYPE],[ITN_SYMBOL],[ITN_WHF] " +
                           " ,[ITN_WHT],[ITN_DESC],[ITN_GOODS],[ITN_QTY] " +
                           " ,[ITN_UM],[ITN_STOCKQTY],[ITN_STOCKKUM],[ITN_VALUE] " +
                           " ,[ITN_USER],[ITN_UDATE]) " +
                           " VALUES(0,'22','" + itnDocNo + "','" + itnDate.ToString("yyyy/MM/dd") + "'," +
                           itnLine.ToString () + ", 9 ," + iSymbol + " ,"+ itnWhf + "','" +
                           "" + itnWht + "','" + itnDesc + "','" + itnGoods + "'," + itnQty.ToString () + "," +
                           "'"+ itnUm + "'," + itnStockQty.ToString() +",'" + itnStockUm + "', 0 ," +
                           "'SAPTR',getdate() " +
                           ")";
            exeSml(lSql);

        }

        private void exeSql(string lSql)
        {
           using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = sqlCon;
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd = new SqlCommand(lSql, conn);

                sqlCmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        private void exeSml(string lSql)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = smlCon;
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd = new SqlCommand(lSql, conn);

                sqlCmd.ExecuteNonQuery();

                conn.Close();
            }
        }
        private void btVanTrClict(object sender, RoutedEventArgs e)
        {
            exeSml("delete TB_ITRAN where ITN_USER='SAPTR' and ITN_DOCNO ='" + txtDocNo.Text + "'" );

            HanaDataAdapter dta = new HanaDataAdapter();
            DataTable dt = new DataTable();
            
            string lSql = sapSql();

            dta = new HanaDataAdapter(lSql, sapConn());
            dta.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //insert TB_ITRAN
                string itnDocno = dt.Rows[i]["ITN_DOCNO"].ToString() ;
                DateTime itnDate = DateTime.Parse(dt.Rows[i]["ITN_DATE"].ToString());
                int itnLine = int.Parse(dt.Rows[i]["ITN_LINE"].ToString());
                string itnWhf = dt.Rows[i]["ITN_WHF"].ToString();
                string itnWht = dt.Rows[i]["ITN_WHT"].ToString();
                string itnDesc = dt.Rows[i]["ITN_DESC"].ToString();
                string intGoods = dt.Rows[i]["ITN_GOODS"].ToString();
                double itnQty = double.Parse(dt.Rows[i]["ITN_QTY"].ToString());
                string itnUm = dt.Rows[i]["ITN_UM"].ToString();
                double itnStockQty = double.Parse(dt.Rows[i]["ITN_STOCKQTY"].ToString());
                string itnStockUm = dt.Rows[i]["ITN_STOCKUM"].ToString();
                try
                {
                    //TRANSFER OUT
                    insertTBITRAN(itnDocno,
                                  itnDate,
                                  itnLine - 1,
                                  itnWhf,
                                  itnWht,
                                  itnDesc,
                                  intGoods,
                                  itnQty * -1,
                                  itnUm,
                                  itnStockQty * -1,
                                  itnStockUm
                                  );
                    //TRANSFER IN
                    insertTBITRAN(itnDocno,
                                  itnDate,
                                  itnLine,
                                  itnWht,
                                  itnWhf,
                                  itnDesc,
                                  intGoods,
                                  itnQty,
                                  itnUm,
                                  itnStockQty,
                                  itnStockUm
                                  );
                    
                }
                catch (Exception ex) {
                    //
                    MessageBox.Show(ex.Message);
                }
                //MessageBox.Show(dt.Rows[i]["ITN_GOODS"].ToString()); 
                
            }
            btnToSml.Visibility = Visibility.Hidden ;
            MessageBox.Show("Ok");
        }

        private string sapSql()
        {
            string lSql = "select  " +
                          "     '0'      ITN_STS, " +
                          "     '22' ITN_IDOCTYPE,	 " +
                          "      SUBSTRING(IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\"),0,7)  ITN_DOCNO,  " +
                          "      T1.\"DocDate\"	ITN_DATE,  " +
                          "      ((T0.\"LineNum\" + 1)*10)+1	ITN_LINE,  " +
                          "      '9' ITN_DOCTYPE,  " +
                          "      '1' ITN_SYMBOL,  " +
                          "      CASE T0.\"WhsCode\"  " +
                          "      WHEN    'A01'   THEN    '1A'  " +
                          "      WHEN    'A02'   THEN    '1B'  " +
                          "      WHEN    'A03'   THEN    '1C'   " +
                          "      WHEN    'A04'   THEN    '1D'   " +
                          "      WHEN    'A05'   THEN    '1E'    " +
                          "      WHEN    'A06'   THEN    '1F'     " +
                          "      WHEN    'B01'   THEN    '2A'   " +
                          "      WHEN    'B02'   THEN    '2B'    " +
                          "      WHEN    'B03'   THEN    '2C' " +
                          "      WHEN    'B04'   THEN    '2D'  " +
                          "      WHEN    'B05'   THEN    '2E'   " +
                          "      WHEN    'B06'   THEN    '2F'   " +
                          "      WHEN    'B07'   THEN    '2G'   " +
                          "      WHEN    'B08'   THEN    '2H'   " +
                          "      WHEN    'B09'   THEN    '2I'   " +
                          "      WHEN    'B10'   THEN    '2J'   " +
                          "      WHEN    'B11'   THEN    '2K'   " +
                          "      WHEN    'C01'   THEN    '2L'   " +
                          "      WHEN    'C02'   THEN    '2M'   " +
                          "      WHEN    'C03'   THEN    '2N'   " +
                          "      WHEN    'C04'   THEN    '2O'   " +
                          "      WHEN    'B12'   THEN    '2P'   " +
                          "      WHEN    'X00'   THEN    '2Q'   " +
                          "      WHEN    'B13'   THEN    '2R'   " +
                          "      WHEN    'B14'   THEN    '2S'    " +
                          "      WHEN    'B15'   THEN    '2T'   " +
                          "      WHEN    'B16'   THEN    '2U'   " +
                          "      WHEN    'B17'   THEN    '2V'   " +
                          "      WHEN    'B18'   THEN    '2W'   " +
                          "      WHEN    'B19'   THEN    '2X'   " +
                          "      WHEN    'B20'   THEN    '2Y'   " +
                          "      WHEN    'B21'   THEN    '2Z'   " +
                          "      WHEN    'B22'   THEN    '3A'   " +
                          "      WHEN    'B23'   THEN    '3B'   " +
                          "      WHEN    'B24'   THEN    '3C'   " +
                          "      END             ITN_WHF,   " +
                          "      T0.\"FromWhsCod\"    ITN_WHT,   " +
                          "      T1.\"Comments\"	ITN_DESC,  " +
                          "      T0.\"ItemCode\"	ITN_GOODS,   " +
                          "      T0.\"Quantity\"	ITN_QTY,   " +
                          "      T0.\"unitMsr\"  ITN_UNIT, " +
                          "      CASE T0.\"unitMsr\" " +
                          "      WHEN 'หีบ' THEN '3' " +
                          "      WHEN 'ลัง' THEN '3' " +
                          "      WHEN 'กป' THEN '1' " +
                          "      WHEN 'ซอง' THEN '1' " +
                          "      WHEN 'กป.' THEN '1' " +
                          "      END ITN_UM,   " +
                          "      T0.\"InvQty\"	ITN_STOCKQTY,   " +
                          "      '1' ITN_STOCKUM,   " +
                          "      '0' ITN_VALUE, " +
                          "      'SAPTR' ITN_USER,   " +
                          "      CURRENT_DATE    ITN_UDATE " +
                          " " +
                   " FROM  \"SBOKPS_LIVE\".\"WTR1\" T0  inner join \"SBOKPS_LIVE\".\"OWTR\" T1 " +
                   "  on T0.\"DocEntry\" = T1.\"DocEntry\"  " +
                   " where 1=1 " +
                   " and T1.\"CANCELED\" = 'N' and T0.\"Quantity\" > 0 " +
                   " and IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\") like '" + txtDocNo.Text + "'" +
                   " and T0.\"DocDate\" >= '20200129' " +
                   " and T0.\"WhsCode\" in ('A01','A02','A03','A04','A05','A06','B01','B02','B03','B04','B05','B06','B07','B08','B09','B10','B11','B12','B13','B14','B15','B16','B17','B18','B19','B20','B21','B22','B23','B24','C01','C02','C03','C04') " +
                   " and IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\") like '%/%' ";
            //" order by T0.\"WhsCode\", T0.\"ItemCode\" ,T0.\"BatchNum \" ";

            return lSql;
        }

        private string sapSqlSolDev()
        {
            string lSql = "select  " +
                          "     CASE T0.\"WhsCode\"  " +
                          "      WHEN    'A01'   THEN    '401'  " +
                          "      WHEN    'A02'   THEN    '402'  " +
                          "      WHEN    'A03'   THEN    '403'   " +
                          "      WHEN    'A04'   THEN    '404'   " +
                          "      WHEN    'A05'   THEN    '405'    " +
                          "      WHEN    'A06'   THEN    '406'     " +
                          "      WHEN    'B01'   THEN    '501'   " +
                          "      WHEN    'B02'   THEN    '502'    " +
                          "      WHEN    'B03'   THEN    '503' " +
                          "      WHEN    'B04'   THEN    '504'  " +
                          "      WHEN    'B05'   THEN    '505'   " +
                          "      WHEN    'B06'   THEN    '506'   " +
                          "      WHEN    'B07'   THEN    '507'   " +
                          "      WHEN    'B08'   THEN    '508'   " +
                          "      WHEN    'B09'   THEN    '509'   " +
                          "      WHEN    'B10'   THEN    '510'   " +
                          "      WHEN    'B11'   THEN    '511'   " +
                          "      WHEN    'C01'   THEN    '531'   " +
                          "      WHEN    'C02'   THEN    '532'   " +
                          "      WHEN    'C03'   THEN    '533'   " +
                          "      WHEN    'C04'   THEN    '534'   " +
                          "      WHEN    'B12'   THEN    '512'   " +
                          "      WHEN    'X00'   THEN    '2Q'   " +
                          "      WHEN    'B13'   THEN    '513'   " +
                          "      WHEN    'B14'   THEN    '514'    " +
                          "      WHEN    'B15'   THEN    '515'   " +
                          "      WHEN    'B16'   THEN    '516'   " +
                          "      WHEN    'B17'   THEN    '517'   " +
                          "      WHEN    'B18'   THEN    '518'   " +
                          "      WHEN    'B19'   THEN    '519'   " +
                          "      WHEN    'B20'   THEN    '520'   " +
                          "      WHEN    'B21'   THEN    '521'   " +
                          "      WHEN    'B22'   THEN    '522'   " +
                          "      WHEN    'B23'   THEN    '523'   " +
                          "      WHEN    'B24'   THEN    '524'   " +
                          "      END      SaleCode, " +
                          "     '02' BranchCode,    " +
                          "      SUBSTRING(IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\"),0,3)  DocNo,  " +
                          "      SUBSTRING(IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\"),5,3)  RefDocNo,  " +
                          "      T1.\"Comments\"  Remark,  " +
                          "      '1' Lot, " +
                          "      T0.\"LineNum\" + 1	Seq,  " +
                          "      T0.\"ItemCode\"	ProductCode,   " +
                          "      CASE T0.\"unitMsr\" " +
                          "      WHEN 'หีบ' THEN '3' " +
                          "      WHEN 'ลัง' THEN '3' " +
                          "      WHEN 'กป' THEN '1' " +
                          "      WHEN 'ซอง' THEN '1' " +
                          "      WHEN 'กป.' THEN '1' " +
                          "      END UnitCode,   " +
                          "      T0.\"Quantity\"	TransQty,   " +
                          "      CONCAT(CONCAT(T0.\"unitMsr\", cast(CAST(T0.\"InvQty\" AS INTEGER) AS VARCHAR(10))) , T0.\"unitMsr2\") RemarkDetail " +
                          " " +
                   " FROM  \"SBOKPS_LIVE\".\"WTR1\" T0  inner join \"SBOKPS_LIVE\".\"OWTR\" T1 " +
                   "  on T0.\"DocEntry\" = T1.\"DocEntry\"  " +
                   " where 1=1 " +
                   " and T1.\"CANCELED\" = 'N' and T0.\"Quantity\" > 0 " +
                   " and IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\") like '" + txtDocNo.Text + "'" +
                   " and T0.\"DocDate\" >= '20200129' " +
                   " and T0.\"WhsCode\" in ('A01','A02','A03','A04','A05','A06','B01','B02','B03','B04','B05','B06','B07','B08','B09','B10','B11','B12','B13','B14','B15','B16','B17','B18','B19','B20','B21','B22','B23','B24','C01','C02','C03','C04') " +
                   " and IFNULL(T1.\"U_ISS_DocRef\",T1.\"Comments\") like '%/%' ";
            //" order by T0.\"WhsCode\", T0.\"ItemCode\" ,T0.\"BatchNum \" ";

            return lSql;
        }
        
        private string sapInvSql()
        {
            string lSql = "select  " +
                          "T0.\"CardCode\" [VAL_CUST] " +
                          ",substring(T0.\"NumAtCard\",1,2) [VAL_PREFIX]  " +
                          ",substring(T0.\"NumAtCard\",3,10) [VAL_INVOICE] " +
                          ",T1.\"LineNum\"+1  [VAL_LINE] " +
                          ",T0.\"DocDate\" [VAL_INVDATE] " +
                          ",'1' [VAL_GOODSTYPE] " +
                          ",T1.\"ItemCode\" [VAL_GOODS] " +
                          ",T1.\"Dscription\" [VAL_DESC] " +
                          ",T1.\"InvQty\"  [VAL_STOCKQTY] " +
                          ",'1' [VAL_STOCKUM] " +
                          ",T1.\"unitMsr\" [VAL_UMDESC] " +
                          ",T1.\"Quantity\" [VAL_QTY] " +
                          ",T1.\"unitMsr2\" [VAL_UM] " +
                          ",[VAL_UMPRICE] " +
                          ",[VAL_UPRICE] " +
                          ",[VAL_AMOUNT] " +
                          ",[VAL_DISC] " +
                          ",[VAL_DISCAMT] " +
                          ",[VAL_TOTAL] " +
                          ",[VAL_AVGDISC] " +
                          ",[VAL_AVGVAT] " +
                          ",[VAL_USER] " +
                          ",[VAL_UDATE] " +
                          ",[VAL_DATE]" +
                    "from \"SBOKPS_LIVE\".\"OINV\" T0 inner join \"SBOKPS_LIVE\".\"INV1\" T1 " +
                    "      on T0.\"DocEntry\" = T1.\"DocEntry\" " +
                    "where T0.\"NumAtCard\" like  '[123]%' " +
                    "  and T0.\"DocDate\" >= '20200201' " +
                    "  and T0.\"Serial\" = 3094 " +
                    "  T1.\"WhsCode\" in ('A01', 'A02', 'A03', 'A04', 'A05', 'A06', 'B01', 'B02', 'B03', 'B04', 'B05', 'B06', 'B07', 'B08', 'B09', 'B10', 'B11', 'B12', 'B13', 'B14', 'B15', 'B16', 'B17', 'B18', 'B19', 'B20', 'B21', 'B22', 'B23', 'B24', 'C01', 'C02', 'C03', 'C04') "; 
                return lSql;

        }
        
        private void CLICK_btVanTr(object sender, RoutedEventArgs e)
        {
            //--Show To SmilingFish
            sapToSmilind();
            //--Show SolDev
            SapToSoldev();
        }

        private void sapToSmilind()
        {
            HanaDataAdapter dta = new HanaDataAdapter();
            DataTable dt = new DataTable();

            string lSql = sapSql();

            HanaCommand cmd = new HanaCommand(lSql, sapConn());
            HanaDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                dgList.ItemsSource = reader;

                btnToSml.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Document not found..!");
            }

        }
        private void btSapInvClict(object sender, RoutedEventArgs e)
        {
            //exeSml("delete TB_ITRAN where VAL_USER='SAPTR'");

            HanaDataAdapter dta = new HanaDataAdapter();
            DataTable dt = new DataTable();

            string lSql = sapSqlArInv();

            dta = new HanaDataAdapter(lSql, sapConn());
            dta.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //insert TB_ITRAN
                string valCUST = dt.Rows[i]["VAL_CUST"].ToString();
                string valPREFIX = dt.Rows[i]["VAL_PREFIX"].ToString();
                string valINVOICE = dt.Rows[i]["VAL_INVOICE"].ToString();
                int valLINE = int.Parse(dt.Rows[i]["VAL_LINE"].ToString());
                DateTime valINVDATE = DateTime.Parse(dt.Rows[i]["VAL_INVDATE"].ToString());
                string valGOODSTYPE = "1"; // dt.Rows[i]["VAL_GOODSTYPE"].ToString();
                string valGOODS = dt.Rows[i]["VAL_GOODS"].ToString();
                string valDESC = dt.Rows[i]["VAL_DESC"].ToString();
                double valSTOCKQTY = double.Parse(dt.Rows[i]["VAL_STOCKQTY"].ToString());
                string valSTOCKUM = dt.Rows[i]["VAL_STOCKUM"].ToString();
                string valUMDESC = dt.Rows[i]["VAL_UMDESC"].ToString();
                double valQTY = double.Parse(dt.Rows[i]["VAL_QTY"].ToString());
                string valUM = dt.Rows[i]["VAL_UM"].ToString();
                double valUMPRICE = double.Parse(dt.Rows[i]["VAL_UMPRICE"].ToString());
                double valUPRICE = double.Parse(dt.Rows[i]["VAL_UPRICE"].ToString());
                double valAMOUNT = double.Parse(dt.Rows[i]["VAL_AMOUNT"].ToString());
                double valDISC = double.Parse(dt.Rows[i]["VAL_DISC"].ToString());
                double valDISCAMT = double.Parse(dt.Rows[i]["VAL_DISCAMT"].ToString());
                double valTOTAL = double.Parse(dt.Rows[i]["VAL_TOTAL"].ToString());
                double valAVGDISC = double.Parse(dt.Rows[i]["VAL_AVGDISC"].ToString());
                double valAVGVAT = double.Parse(dt.Rows[i]["VAL_AVGVAT"].ToString());
                string valUSER = dt.Rows[i]["VAL_USER"].ToString();
                DateTime valUDATE = DateTime.Parse(dt.Rows[i]["VAL_UDATE"].ToString());
                DateTime valDATE = DateTime.Parse(dt.Rows[i]["VAL_DATE"].ToString());
                
                try
                {
                    insertSapVanl(valCUST
                              ,  valPREFIX
                              ,  valINVOICE
                              ,  valLINE
                              , valINVDATE
                              , valGOODSTYPE
                              , valGOODS
                              , valDESC
                              , valSTOCKQTY
                              , valSTOCKUM
                              , valUMDESC
                              , valQTY
                              , valUM
                              , valUMPRICE
                              , valUPRICE
                              , valAMOUNT
                              , valDISC
                              , valDISCAMT
                              , valTOTAL
                              , valAVGDISC
                              , valAVGVAT
                              , valUSER
                              , valUDATE
                              , valDATE );
               
                }
                catch (Exception ex)
                {
                    //
                    MessageBox.Show(ex.Message);
                }
                //MessageBox.Show(dt.Rows[i]["VAL_GOODS"].ToString()); 

            }
            
            MessageBox.Show("Ok");
        
        }

        private void SapToSoldev()
        {
            HanaDataAdapter dta = new HanaDataAdapter();
            DataTable dt = new DataTable();

            string lSql = sapSqlSolDev();

            HanaCommand cmd = new HanaCommand(lSql, sapConn());
            HanaDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                dgSoldev.ItemsSource = reader;
                //btnToSml.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Document not found..!");
            }

        }
  
    
        private string sapSqlArInv()
        {
            string lSql = " select  substring(T0.\"CardCode\",2,5)  VAL_CUST " +
                              "       ,substring(T0.\"NumAtCard\", 0, 2) VAL_PREFIX " +
                              "       ,substring(T0.\"NumAtCard\", 3, 10) VAL_INVOICE " +
                              "       , T1.\"LineNum\" + 1   VAL_LINE " +
                              "       , T0.\"DocDate\"  VAL_INVDATE " +
                              "       , T1.\"ItemCode\"  VAL_GOODS " +
                            "       , T1.\"Dscription\" VAL_DESC " +
                            "       , T1.\"InvQty\" VAL_STOCKQTY " +
                            "       , '1' VAL_STOCKUM " +
                            "       , T1.\"unitMsr2\" VAL_UMDESC " +
                            "       , T1.\"Quantity\"  VAL_QTY " +
                            "       ,CASE T1.\"unitMsr\" " +
                            "      WHEN 'หีบ' THEN '3' " +
                            "      WHEN 'ลัง' THEN '3' " +
                            "      WHEN 'กล่อง' THEN '2' " +
                            "      WHEN 'กป' THEN '1' " +
                            "      WHEN 'ซอง' THEN '1' " +
                            "      WHEN 'กป.' THEN '1' " +
                            "      END VAL_UM " +
                            "       , 0 VAL_UMPRICE " +
                            "       , T1.\"Price\" VAL_UPRICE " +
                            "       , T1.\"LineTotal\" VAL_AMOUNT " +
                            "       ,0    VAL_DISC " +
                            "       ,0    VAL_DISCAMT " +
                            "       , T1.\"LineTotal\"     VAL_TOTAL " +
                            "       ,0   VAL_AVGDISC " +
                               "       ,T1.\"VatSum\" VAL_AVGVAT " +
                            "       ,'SAPINV' VAL_USER " +
                            "       , CURRENT_DATE VAL_UDATE " +
                            "       , CURRENT_DATE VAL_DATE " +
                    " from \"SBOKPS_LIVE\".\"OINV\" T0 inner join \"SBOKPS_LIVE\".\"INV1\" T1  " +
                    "         on T0.\"DocEntry\" = T1.\"DocEntry\"  " +
                    " where  1 = 1  " +
                    //--and T0."NumAtCard" like '2E%'
                    " and T0.\"DocDate\" >= '20200101' " +
                    //--and T1."ItemCode" = '0060'
                    //" and T0.\"Serial\" = '3094' " +
                    " and T0.\"ObjType\" = '13' " +
                    " and T0.\"CANCELED\" <> 'Y' " +
                    " and T1.\"WhsCode\" in ('A01', 'A02', 'A03', 'A04', 'A05', 'A06', 'B01', 'B02', 'B03', 'B04', 'B05', 'B06', 'B07', 'B08', 'B09', 'B10', 'B11', 'B12', 'B13', 'B14', 'B15', 'B16', 'B17', 'B18', 'B19', 'B20', 'B21', 'B22', 'B23', 'B24', 'C01', 'C02', 'C03', 'C04') " +
                    " order by T0.\"NumAtCard\" ";
            return lSql;
        }
    
        private void insertSapVanl(string valCUST
                          ,string valPREFIX
                          ,string valINVOICE
                          ,int valLINE
                          ,DateTime valINVDATE
                          ,string valGOODSTYPE
                          ,string valGOODS
                          ,string valDESC
                          ,double valSTOCKQTY
                          ,string valSTOCKUM
                          ,string valUMDESC
                          ,double valQTY
                          ,string valUM
                          ,double valUMPRICE
                          ,double valUPRICE
                          ,double valAMOUNT
                          ,double valDISC
                          ,double valDISCAMT
                          ,double valTOTAL
                          ,double valAVGDISC
                          ,double valAVGVAT
                          ,string valUSER
                          ,DateTime valUDATE
                          ,DateTime valDATE) {

            string xSql = "INSERT INTO [SAP].[dbo].[SAP_VANL] " +
                           "([VAL_CUST],[VAL_PREFIX],[VAL_INVOICE],[VAL_LINE],[VAL_INVDATE] " +
                           ",[VAL_GOODSTYPE],[VAL_GOODS],[VAL_DESC],[VAL_STOCKQTY],[VAL_STOCKUM] " +
                           ",[VAL_UMDESC],[VAL_QTY],[VAL_UM],[VAL_UMPRICE],[VAL_UPRICE] " +
                           ",[VAL_AMOUNT],[VAL_DISC],[VAL_DISCAMT],[VAL_TOTAL],[VAL_AVGDISC] " +
                           ",[VAL_AVGVAT],[VAL_USER],[VAL_UDATE],[VAL_DATE]) " +
                           " VALUES('"+  valCUST + "','" + valPREFIX + "','" + valINVOICE + "'," + valLINE + ",'" + valINVDATE + "'" +
                           "    ,'" + valGOODSTYPE + "','" + valGOODS + "','" + valDESC + "'," + valSTOCKQTY + ",'" + valSTOCKUM + "'" +
                           "    ,'" + valUMDESC + "'," + valQTY + ",'" + valUM + "'," + valUMPRICE + "," + valUPRICE + "" +
                           "    ," + valAMOUNT + "," + valDISC + "," + valDISCAMT + "," + valTOTAL + "," + valAVGDISC +  "" +
                           "    ," + valAVGVAT + ",'" + valUSER + "','" + valUDATE + "','" + valDATE + "'); ";
            exeSql(xSql);


        }
    }
}

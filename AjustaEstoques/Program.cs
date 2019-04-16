using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.DataAccess.Client;
using AjustaEstoques.Banco.SqlConn;
using System.Data;
using AjustaEstoques.Banco.Exceptions;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using AjustaEstoques.Entities;

namespace AjustaEstoques
{
    class Program
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {

            DateTime d1 = DateTime.Now;
            int hora, minuto;
            List<Product> products = new List<Product>();


            try
            {
            Console.WriteLine("Digite a hora: ");
            hora = int.Parse(Console.ReadLine());
            Console.WriteLine("Digite os minutos: ");
            minuto = int.Parse(Console.ReadLine());
            Console.WriteLine("Dê enter para iniciar o programa!");
            Console.ReadLine();
            
                do
                {
                    
                    IntPtr hWnd = GetConsoleWindow();
                    ShowWindow(hWnd, 0);
                    do
                    {
                        d1 = DateTime.Now;

                    } while (!(d1.Hour == hora && d1.Minute == minuto));

                    string data = "'" + d1.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture) + "'";
                    ShowWindow(hWnd, 1);

                    OracleConnection conn = DBUtils.GetDBConnection();
                    OracleCommand cmd = new OracleCommand();
                    Console.WriteLine("Get Connection: " + conn);

                    conn.Open();
                    Console.WriteLine("Conexão bem sucedida");


                    string consulta = "SELECT ESTOQUE.CODPROD ,ESTOQUE.QTESTGER ,VENDAS.GIRO ,ESTOQUE.QTESTGER - VENDAS.GIRO SALDO_FINAL FROM (SELECT CODPROD, QTESTGER " +
                      "FROM PCEST " +
                      "WHERE CODFILIAL = 1 AND CODPROD IN(SELECT A.CODPROD FROM PCMOV A, PCNFSAID B WHERE A.NUMTRANSVENDA = B.NUMTRANSVENDA " +
                      "AND A.DTMOV = " + data + " " +
                      "AND A.CODFILIAL = 50 " +
                      "AND A.CODOPER = 'S' " +
                      "AND B.DTCANCEL IS NULL)) ESTOQUE, " +
                      "(SELECT A.CODPROD,SUM(A.QT) GIRO " +
                      "FROM PCMOV A, PCNFSAID B " +
                      "WHERE A.NUMTRANSVENDA = B.NUMTRANSVENDA " +
                        "AND A.DTMOV = " + data + " " +
                        "AND A.CODFILIAL = 50 " +
                        "AND A.CODOPER = 'S' " +
                        "AND B.DTCANCEL IS NULL " +
                        "GROUP BY CODPROD) VENDAS " +
                        "WHERE ESTOQUE.CODPROD = VENDAS.CODPROD " +
                        "ORDER BY VENDAS.GIRO DESC ";


                    using (var comm = new Oracle.DataAccess.Client.OracleCommand())
                    {
                        comm.CommandText = consulta;
                        comm.Connection = conn;
                        using (var reader = comm.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var codprodVar = reader["CODPROD"];
                                int codprod = (int)codprodVar;
                                var qtestgerVar = reader["QTESTGER"];
                                decimal qtestger = (decimal)qtestgerVar;
                                var giroVar = reader["GIRO"];
                                decimal giro = (decimal)giroVar;
                                var saldo_finalVar = reader["SALDO_FINAL"];
                                decimal saldoFinal = (decimal)saldo_finalVar;
                                products.Add(new Product(codprod, qtestger, giro, saldoFinal));

                            }
                        }
                    }







                    using (var comm = new Oracle.DataAccess.Client.OracleCommand())
                    {
                        comm.Connection = conn;

                        comm.CommandText = "UPDATE PCEST SET QTESTGER = QTESTGER - (SELECT SUM(A.QT) FROM PCMOV A, PCNFSAID B WHERE A.NUMTRANSVENDA = B.NUMTRANSVENDA AND A.DTMOV = :Data AND A.CODFILIAL = 50 AND A.CODOPER = 'S' AND B.DTCANCEL IS NULL AND A.CODPROD = PCEST.CODPROD) WHERE CODFILIAL = 1 AND CODPROD IN(SELECT A.CODPROD FROM PCMOV A, PCNFSAID B WHERE A.NUMTRANSVENDA = B.NUMTRANSVENDA AND A.DTMOV = :Data AND A.CODFILIAL = 50 AND A.CODOPER = 'S' AND B.DTCANCEL IS NULL) ";
                        comm.Parameters.Add("Data", data);
                        comm.ExecuteNonQuery();
                    }





                    using (var comm = new Oracle.DataAccess.Client.OracleCommand())
                    {
                        comm.CommandText = consulta;
                        comm.Connection = conn;
                        using (var reader = comm.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var codprod = reader["CODPROD"];
                                var qtestger = reader["QTESTGER"];
                                var giro = reader["GIRO"];
                                var saldo_final = reader["SALDO_FINAL"];



                                if (qtestger != saldo_final)
                                {

                                    using (StreamWriter sw = File.AppendText("entrada2.txt"))
                                    {
                                        sw.WriteLine(" ---------------------------  Execução com erro: " + DateTime.Now + " -----------------------------");
                                        sw.WriteLine("Erro no codigo: " + codprod);
                   
                                    }
                                    foreach (Product prod in products)
                                    {
                                        using (var comm2 = new Oracle.DataAccess.Client.OracleCommand())
                                        {
                                            comm2.Connection = conn;

                                            comm2.CommandText = "UPDATE PCEST SET QTESTGER = :Qtestger where codprod = :Codprod and codfilial = 1";
                                            comm2.Parameters.Add("Qtestger", prod.Qtestger);
                                            comm2.Parameters.Add("Codprod", prod.Codprod);
                                            comm2.ExecuteNonQuery();
                                        }

                                    }





                                    break;

                                }
                                using (StreamWriter sw = File.AppendText("entrada2.txt"))
                                {
                                    sw.WriteLine(" ---------------------------  Execução completa: " + DateTime.Now + " -----------------------------");
                                    foreach (Product prod in products)
                                    {
                                        sw.WriteLine(prod);
                                    }
                                }
                            }
                        }

                    }


                    products.Clear();


                    do
                    {
                        d1 = DateTime.Now;

                    } while (d1.Hour == hora && d1.Minute == minuto);
                    

                } while (true);


                    

            }
            catch (Exception ex)
            {
                Console.WriteLine("## ERROR: " + ex.Message);
                Console.Read();
                return;
            }



        }
    }
}

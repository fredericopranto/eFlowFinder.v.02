using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

namespace eFlowFinder
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var pathDestino = @"C:\Users\fredericopranto\Downloads\DIFF\[jboss501ga][jboss510b1][analisado_par].csv";
            var pathOrigem = @"C:\Users\fredericopranto\Downloads\DIFF\ChangeImpact[Manual].csv";

            Console.WriteLine("Lendo arquivo....");
            SerachInFile(pathOrigem, pathDestino);
            Console.WriteLine("----------OK-------------");
        }

        public static void SerachInFile(string pathOrigem, string pathDestino)
        {
            var readerFile = new StreamReader(
                   File.OpenRead(pathOrigem));

            int indice = 0;
            string[] lines = File.ReadAllLines(pathOrigem);

            while (!readerFile.EndOfStream)
            {
                var line = readerFile.ReadLine().Split(';');
                var valor = line[0];
                if (valor.Contains("("))
                {
                    valor = valor.ToString().Substring(0, valor.ToString().IndexOf('('));    
                }
                var ClasseComMetodo = valor.Split('.');

                var metodo = "";
                var classe = "";

                if (ClasseComMetodo.Length > 1)
                {
                    metodo = ClasseComMetodo[ClasseComMetodo.Length - 1];
                    classe = ClasseComMetodo[ClasseComMetodo.Length - 2];
                }
                else
                {
                    if (ClasseComMetodo[0].Contains("("))
                        metodo = ClasseComMetodo[0];
                    else
                        classe = ClasseComMetodo[0];
                }

                FindInFile(pathDestino, pathOrigem, classe.Trim(), metodo.Trim(), indice, ref lines);

                indice++;
            }


            File.WriteAllLines(@"C:\Users\fredericopranto\Downloads\DIFF\ChangeImpact[Automatico].csv", lines);

        }

        public static void FindInFile(string pathDestino, string pathOrigem, string className, string method, int indice, ref string[] lines)
        {
            ArrayList linhasOk = new ArrayList();

            var readerFile = new StreamReader(
                   File.OpenRead(pathDestino));

            while (!readerFile.EndOfStream)
            {
                var line = readerFile.ReadLine();
                var hasvalues = line.Contains("." + className + ":") && line.Contains(" " + method + "(");
                if (hasvalues)
                {
                    linhasOk.Add(line);
                    Console.WriteLine(line);
                }
            }

            if (linhasOk.Count == 1)
            {
                EditaArquivo(pathOrigem, linhasOk, indice, ref lines);
            }
            if (linhasOk.Count > 1)
            {
                SelecionaValor(pathOrigem, linhasOk, indice, ref lines);
            }

        }

        public static void SelecionaValor(string pathOrigem, ArrayList linhasOk, int indice, ref string[] lines)
        {
            ArrayList opa = new ArrayList();

            foreach (var item in linhasOk)
            {
                var itemteste = "";
                
                if (item.ToString().Contains("("))
                {
                    itemteste = item.ToString().ToString().Substring(0, item.ToString().ToString().IndexOf('('));
                }

                if (!itemteste.ToString().Contains("$"))
                {
                    string[] itens = item.ToString().Split(';');
                    int count = Regex.Matches(itens[2], ",").Count;
                    bool metodovazio = itens[2].Contains("()");

                    string[] itens3 = lines[indice].ToString().Split(';');
                    int count3 = Regex.Matches(itens3[0], ",").Count;
                    bool metodovazio3 = itens3[0].Contains("()");

                    if (metodovazio && metodovazio3)
                    {
                        opa.Add(item);
                        break;
                    }


                    if (count == count3)
                    {
                        opa.Add(item);
                    }
                }
            }

            if (opa.Count == 1)
            {
                EditaArquivo(pathOrigem, opa, indice, ref lines);
            }
            
            if (opa.Count > 1)
            {
                var parametrosOrigem = "";
                var parametrosDestino = "";
                if (lines[indice].ToString().Contains("("))
                {
                    parametrosOrigem = lines[indice].ToString().ToString().Substring(lines[indice].ToString().ToString().IndexOf('('));
                }


                //TODO melhorar o codigo..procurar por todos os tipos nos parametros
                foreach (var item in linhasOk)
                {
                    if (item.ToString().Contains("("))
                    {
                        parametrosDestino = item.ToString().ToString().Substring(item.ToString().ToString().IndexOf('('));
                    }

                    var comparar = "";
                    if (parametrosOrigem.IndexOf(' ') != -1)
                    {
                        comparar = parametrosOrigem.Substring(1, parametrosOrigem.IndexOf(' '));
                    }
                    else
                    {
                        int i = 0;
                    }


                    if (parametrosDestino.Contains(comparar.Trim()))
                    {
                        EditaArquivo(pathOrigem, opa, indice, ref lines);
                        break;
                    }
                }

            }
            
        }

        public static string[] EditaArquivo(string pathOrigem, ArrayList linhasOk, int indice, ref string[] lines)
        {
            string[] splitLines = lines[indice].ToString().Split(';');
            string[] splitLinha = linhasOk[0].ToString().Split(';');
            string[] valores = { splitLinha[6], splitLinha[7], splitLinha[3], splitLinha[4], splitLinha[5] };

            string[] x = splitLines, y = valores;
            string[] z = x.Concatenar(y);
            lines[indice] = String.Join(";", z);

            return lines[indice].Split(';');
        }

        public static T[] Concatenar<T>(this T[] x, T[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
    }

    public class StringSearch
    {   
        private readonly string value;
        private readonly List<int> indexList = new List<int>();
        public StringSearch(string value)
        {
            this.value = value;
        }
        public bool Found(int nextChar)
        {
            for (int index = 0; index < indexList.Count; )
            {
                int valueIndex = indexList[index];
                if (value[valueIndex] == nextChar)
                {
                    ++valueIndex;
                    if (valueIndex == value.Length)
                    {
                        indexList[index] = indexList[indexList.Count - 1];
                        indexList.RemoveAt(indexList.Count - 1);
                        return true;
                    }
                    else
                    {
                        indexList[index] = valueIndex;
                        ++index;
                    }
                }
                else
                {   // next char does not match
                    indexList[index] = indexList[indexList.Count - 1];
                    indexList.RemoveAt(indexList.Count - 1);
                }
            }
            if (value[0] == nextChar)
            {
                if (value.Length == 1)
                    return true;
                indexList.Add(1);
            }
            return false;
        }
        public void Reset()
        {
            indexList.Clear();
        }

        public static DataTable ConvertExcelToDataTable(string FilePath)
        {
            string strConn = string.Empty;

            if (FilePath.Trim().EndsWith(".xlsx"))
            {
                strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", FilePath);
            }
            else if (FilePath.Trim().EndsWith(".xls"))
            {
                strConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";", FilePath);
            }

            OleDbConnection conn = null;
            OleDbCommand cmd = null;
            OleDbDataAdapter da = null;
            DataTable dt = new DataTable();
            try
            {
                conn = new OleDbConnection(strConn);
                conn.Open();
                cmd = new OleDbCommand(@"SELECT * FROM [Sheet1$]", conn);
                cmd.CommandType = CommandType.Text;
                da = new OleDbDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Console.ReadLine();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Dispose();
                cmd.Dispose();
                da.Dispose();
            }
            return dt;
        }
    }
}

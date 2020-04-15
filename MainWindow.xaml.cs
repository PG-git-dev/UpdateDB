using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using DevExpress.Spreadsheet;
using DevExpress.Spreadsheet.Export;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace BaseUpdate
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IniFile INI = new IniFile("dbConf.ini");
        //string connStr = @"Data Source=DESKTOP-N6R6KEU\SQLEXPRESS;Initial Catalog=STD_DB_DICT_HIVE;Integrated Security=True";
        //string connStr = @"Data Source=SQLSRV2017\SQL2017;Initial Catalog=TEST_STD_DB_DICT_HIVE;Integrated Security=True";
        DataTable dtDb = new DataTable();
        DataTable changesTable = new DataTable();
        DataTable excelDataTable = new DataTable();
        string fileName;


        public MainWindow()
        {
            InitializeComponent();
            //if (INI.KeyExists("Source", "DB_Connection") && INI.KeyExists("Source", "Catalog"))
            //string connStr = $"Data Source={INI.ReadINI("DB_Connection", "Source")};Initial Catalog={INI.ReadINI("DB_Connection", "Catalog")};Integrated Security=True";
            //if (INI.KeyExists("Source", "DB_Connection"))
            //    System.Windows.MessageBox.Show(INI.ReadINI("DB_Connection", "Source"));
            grid.Visibility = Visibility.Collapsed;
            excelDataTable.Columns.Add("numberrrr");
            excelDataTable.Columns.Add("code");
            excelDataTable.Columns.Add("name");
            excelDataTable.Columns.Add("cost");
            excelDataTable.Columns["cost"].DataType = System.Type.GetType("System.Double");
            //if (changesTable.Columns.Count < 1)
            //{
            //    changesTable.Columns.Add("code");
            //    changesTable.Columns.Add("old_cost");
            //    changesTable.Columns.Add("new_cost");
            //    changesTable.Columns.Add("old_name");
            //    changesTable.Columns.Add("new_name");
            //    changesTable.Columns.Add("file_n");
            //    changesTable.Columns.Add("difference");
            //}

        }

        private void MedStuffUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            medStuffUpdateButton.IsEnabled = false;
            dtDb.Clear();
            excelDataTable.Clear();
            changesTable.Clear();

            #region File choise
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XLSX-files|*.xlsx";
            openFileDialog.ShowDialog();
            #endregion

            #region Range search
            Workbook wbook = new Workbook();
            if (openFileDialog.FileName == "")
                System.Windows.MessageBox.Show("Выберите файл", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                wbook.LoadDocument(openFileDialog.FileName);
                DevExpress.Spreadsheet.Worksheet worksheet =
                    wbook.Worksheets[0];
                int lastRow = 0;
                while (!worksheet.Cells[lastRow, 1].Value.IsEmpty)
                {
                    lastRow++;
                }
                //var range = worksheet.Tables[0].Range;
                var range = worksheet.Range[$"A1:D{lastRow}"].CurrentRegion;
                #endregion

                #region Excel datatable definition
                //excelDataTable = worksheet..CreateDataTable(range, true);
                ////excelDataTable.Columns[1].DataType = System.Type.GetType("System.String");
                //excelDataTable.Columns[0].ColumnName = "numberrrr";
                //excelDataTable.Columns[1].ColumnName = "code";
                //excelDataTable.Columns[2].ColumnName = "name";
                //excelDataTable.Columns[3].ColumnName = "cost";
                #endregion

                DataTableExporter exporter = worksheet.CreateDataTableExporter(range, excelDataTable, true);
                //exporter.CellValueConversionError += exporter_CellValueConversionError;
                //MyConverter myconverter = new MyConverter();
                //exporter.Options.CustomConverters.Add("As Of", myconverter);
                //// Set the export value for empty cell.
                //myconverter.EmptyCellValue = "N/A";
                exporter.Options.ConvertEmptyCells = true;
                exporter.Options.DefaultCellValueToColumnTypeConverter.SkipErrorValues = false;
                exporter.Export();
                //grid.ItemsSource = excelDataTable;
                //grid.Visibility = Visibility.Visible;

                //DataTable dtDb = new DataTable();
                //DataTable changesTable = new DataTable();

                #region Datatable with changes definition
                //if (changesTable.Columns.Count < 1)
                //{
                //    changesTable.Columns.Add("code");
                //    changesTable.Columns.Add("old_cost");
                //    changesTable.Columns.Add("new_cost");
                //    changesTable.Columns.Add("old_name");
                //    changesTable.Columns.Add("new_name");
                //    changesTable.Columns.Add("file_n");
                //    changesTable.Columns.Add("difference");
                //}
                #endregion

                //string connStr = @"Data Source=DESKTOP-N6R6KEU\SQLEXPRESS;Initial Catalog=STD_DB_DICT_HIVE;Integrated Security=True"; 

                #region Connection to DB
                if (INI.KeyExists("Source", "DB_Connection") && INI.KeyExists("Catalog", "DB_Connection"))
                {
                    try
                    {
                        string connStr = $"Data Source={INI.ReadINI("DB_Connection", "Source")};Initial Catalog={INI.ReadINI("DB_Connection", "Catalog")};Integrated Security=True";
                        using (SqlConnection connection = new SqlConnection(connStr))
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter();
                            //SqlDataAdapter adapter2 = new SqlDataAdapter();
                            adapter.SelectCommand = new SqlCommand("SELECT * FROM med_stuf", connection);
                            adapter.Fill(dtDb);
                            adapter.SelectCommand = new SqlCommand("SELECT * FROM log.med_stuf", connection);
                            adapter.Fill(changesTable);
                            changesTable.Clear();
                            if (changesTable.Columns.Count < 9)
                                changesTable.Columns.Add("difference");
                            //gridControl1.DataSource = dtDb;
                            #region Filling datatable with changes
                            foreach (DataRow newDataRow in excelDataTable.Rows)
                            {
                                if (!newDataRow["code"].Equals(DBNull.Value))
                                {
                                    if (!newDataRow["name"].Equals(DBNull.Value))
                                    {
                                        foreach (DataRow dbRow in dtDb.Select($"code={newDataRow["code"]}"))
                                        {
                                            object oldPrice = DBNull.Value;
                                            object oldName = DBNull.Value;
                                            object newPrice = DBNull.Value;
                                            object newName = DBNull.Value;

                                            if (!dbRow["cost"].Equals(newDataRow["cost"]))
                                            {
                                                oldPrice = dbRow["cost"];
                                                dbRow["cost"] = newDataRow["cost"];
                                                newPrice = newDataRow["cost"];
                                            }
                                            if (!dbRow["name"].Equals(newDataRow["name"]))
                                            {
                                                oldName = dbRow["name"];
                                                dbRow["name"] = newDataRow["name"];
                                                newName = newDataRow["name"];
                                            }
                                            if (!oldName.Equals(newName) || !oldPrice.Equals(newPrice))
                                                changesTable.Rows.Add(new Object[] { newDataRow["code"],
                                                                                oldPrice,
                                                                                newPrice,
                                                                                oldName,
                                                                                newName,
                                                                                fileName,
                                                                                DBNull.Value,
                                                                                DBNull.Value,
                                                                                Convert.ToDouble(newPrice)-Convert.ToDouble(oldPrice)
                                                                                });
                                        }
                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show($"Наименования услуг в файле {openFileDialog.FileName} должны быть заполнены", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                                        changesTable.Clear();
                                    }
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show($"Коды услуг в файле {openFileDialog.FileName} должны быть заполнены", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    changesTable.Clear();
                                }
                            }
                            #endregion
                            if (changesTable.Rows.Count > 0)
                            {
                                grid.ItemsSource = changesTable;
                                grid.Visibility = Visibility.Visible;
                                OkButton.IsEnabled = true;
                                CancelButton.IsEnabled = true;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show($"В файле {openFileDialog.FileName} нет изменений", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
                                medStuffUpdateButton.IsEnabled = true;
                            }
                        }//connection

                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Проверьте параметры подключения к базе данных");
                        medStuffUpdateButton.IsEnabled = true;
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("Проверьте наличие файла конфигурации подключкния к БД", "Ошибка подключения к БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            #endregion
        }//mainButtonClick

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (INI.KeyExists("Source", "DB_Connection") && INI.KeyExists("Catalog", "DB_Connection"))
            {
                string connStr = $"Data Source={INI.ReadINI("DB_Connection", "Source")};Initial Catalog={INI.ReadINI("DB_Connection", "Catalog")};Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM dbo.med_stuf", connection);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(dtDb);
                    //dtDb.Clear();
                    //adapter.Fill(dtDb);
                    //adapter.SelectCommand = new SqlCommand("SELECT * FROM log.med_stuf", connection);
                    string insertString = $"INSERT INTO log.med_stuf (code, old_cost, new_cost, old_name, new_name, file_n) VALUES (@p1, @p2, @p3, @p4, @p5, @p6)";
                    adapter.InsertCommand = new SqlCommand(insertString, connection);
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p1", SqlDbType.NVarChar, 10, "code"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p2", SqlDbType.Float, 10, "old_cost"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p3", SqlDbType.Float, 10, "new_cost"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p4", SqlDbType.NVarChar, 1000, "old_name"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p5", SqlDbType.NVarChar, 1000, "new_name"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@p6", SqlDbType.NVarChar, 100, "file_n"));

/*
                    // устанавливаем команду на вставку
                    adapter.InsertCommand = new SqlCommand("Get_file_name", connection);
                    // это будет зранимая процедура
                    adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                    //// добавляем параметр для name
                    //adapter.InsertCommand.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 50, "Name"));
                    //// добавляем параметр для age
                    //adapter.InsertCommand.Parameters.Add(new SqlParameter("@age", SqlDbType.Int, 0, "Age"));
                    // добавляем выходной параметр для id
                    SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@file_name", SqlDbType.NVarChar, 100, "file_n");
                    parameter.Direction = ParameterDirection.Output;
*/
                    //string sqlExpression = $"CREATE TABLE ##tempTable(name_f NVARCHAR(100) INSERT INTO #tempTable) VALUES ({fileName})";
                    //SqlCommand command = new SqlCommand(sqlExpression, connection);


                    //SqlCommandBuilder scb = new SqlCommandBuilder(adapter);
                    //System.Windows.MessageBox.Show(scb.GetInsertCommand().CommandText);
                    changesTable.Columns.Remove("difference");
                    adapter.Update(changesTable);
                    //label.Content = "База данных обновлена";
                    System.Windows.MessageBox.Show("База данных обновлена");
                    OkButton.IsEnabled = false;
                    CancelButton.IsEnabled = false;
                    medStuffUpdateButton.IsEnabled = true;
                    changesTable.Clear();
                    dtDb.Clear();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Проверьте наличие файла конфигурации подключкния к БД", "Ошибка подключения к БД",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Ну, ок...");
            //label.Content = "Ну, ок...";
            changesTable.Clear();
            grid.Visibility = Visibility.Collapsed;
            OkButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            medStuffUpdateButton.IsEnabled = true;
            dtDb.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConnSettWindow csw = new ConnSettWindow();
            csw.Show();
        }
    }//window
}

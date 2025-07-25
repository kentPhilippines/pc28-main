using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CommonLibrary
{
    public class ExcelUtil
    {
        public static string ExportDataGridViewToExcel(DataGridView dgv, string sheetName, bool filterDummy=false)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                // 写入标题
                int hiddenCount = 0;
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    if (dgv.Columns[i].Visible == false)
                    {
                        hiddenCount++;
                        continue;
                    }
                    worksheet.Cell(1, i - hiddenCount + 1).Value = dgv.Columns[i].HeaderText;
                }

                // 写入数据
                for (int row = 0; row < dgv.RowCount; row++)
                {
                    if (filterDummy)
                    {
                        if((bool)dgv["是否假人", row].Value)
                        {
                            continue;
                        }
                    }
                    hiddenCount = 0;
                    for (int col = 0; col < dgv.ColumnCount; col++)
                    {
                        if (dgv.Rows[row].Cells[col].Visible == false)
                        {
                            hiddenCount++;
                            continue;
                        }
                        worksheet.Cell(row + 2, col - hiddenCount + 1).Value = XLCellValue.FromObject(dgv[col, row].Value);
                    }
                }

                // 调整列宽
                //worksheet.Columns().AdjustToContents();

                string path = IMConstant.UserDataPath + @"\数据导出\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = path + sheetName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                // 保存Excel文件
                workbook.SaveAs(fileName);
                return fileName;
            }
        }

        /// <summary>
        /// 打开文件所在文件夹
        /// </summary>
        /// <param name="filePath"></param>
        public static void OpenFileFolder(string filePath)
        {
            if (File.Exists(filePath))
            {
                string folderPath = Path.GetDirectoryName(filePath);
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
            else
            {
                throw new FileNotFoundException("文件不存在", filePath);
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        public static void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = folderPath,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        public static void ImportExcelToDataGridView(string excelFilePath, DataGridView dgv)
        {
            var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheets.First(); // 获取第一个工作表
            var range = worksheet.RangeUsed(); // 获取已使用的范围（包括标题和数据）
            dgv.Rows.Clear();
            foreach (var row in range.RowsUsed().Skip(1)) // 跳过第一行（标题行）并读取数据行
            {
                object[] dataRow = new object[row.CellsUsed().Count()];
                int colIndex = 0;
                foreach (var cell in row.CellsUsed())
                {
                    dataRow[colIndex++] = cell.Value; // 将值赋给DataTable的行对象。注意列索引递增。
                }
                dgv.Rows.Add(dataRow);
            }
        }
    }
}

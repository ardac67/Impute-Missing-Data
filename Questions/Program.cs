using Questions;
using System.Text;

using (var reader = new StreamReader(@"C:\V\country_vaccination_stats.csv"))
{
    int i = 0;
    List<DataforVaccination> listAll = new List<DataforVaccination>();
    while (!reader.EndOfStream)
    {
        
        DataforVaccination vaccineData=new DataforVaccination();
        var line = reader.ReadLine();
        var values = line.Split(',');
        if(i==0)
        {
            i++;
            continue;
        }
        else
        {
            
            vaccineData.country = values[0];
            vaccineData.date = values[1];

            if (values[2] == "")
            {
                values[2] = "0";
                vaccineData.daily_vaccinations = Int32.Parse(values[2]);
            }
            else vaccineData.daily_vaccinations = Int32.Parse(values[2]);

            vaccineData.vaccines = values[3];
            listAll.Add(vaccineData);
        }
        
    }
    List<int> MissingIndexes = new List<int>();
    MissingIndexes=FindMissing(listAll);
    FindAndFillDataWithMinValue(MissedCountryName(listAll, MissingIndexes),listAll,MissingIndexes);
    ExportCsv(listAll,"imputedVersion");
    

}

static List<int> FindMissing(List<DataforVaccination> listData)
{
    List<int> MissingIndexes = new List<int>();

    for(int i=0;i<listData.Count;i++)
    {
        if (listData[i].daily_vaccinations==0)
        {
            MissingIndexes.Add(i);
        }
    }

    return MissingIndexes;
}
static List<string> MissedCountryName(List<DataforVaccination> listData, List<int> MissingIndexes)
{
    List<string> MissingCountries = new List<string>();
    foreach (var item in MissingIndexes) 
    {
        MissingCountries.Add(listData[item].country);
    }
    return MissingCountries;
}
void FindAndFillDataWithMinValue(List<string> MissingCountries,List<DataforVaccination> listData,List<int> MissingIndexes)
{
    bool control = true;
    foreach(var item in MissingCountries)
    {
        foreach(var item2 in listData)
        {
            if (item == item2.country)
            {
                var validate= (from x in listData
                               where x.country == item && x.daily_vaccinations != 0
                               orderby x.daily_vaccinations ascending
                               select x.daily_vaccinations).Count();
                var min = (from x in listData
                                          where x.country == item && x.daily_vaccinations != 0
                                          orderby x.daily_vaccinations ascending
                                          select x.daily_vaccinations).FirstOrDefault();
                for(int i=0;i<MissingIndexes.Count;i++)
                {
                    if (listData[MissingIndexes[i]].country==item)
                    {
                        if (validate != 0) listData[MissingIndexes[i]].daily_vaccinations = min;
                        else listData[MissingIndexes[i]].daily_vaccinations = 0;
                    }
                }
                control = false;
                break;
            }
        }
        if(control)
        {
            break;
        }
    }
}
static void ExportCsv<T>(List<T> genericList, string fileName)
{
    var sb = new StringBuilder();
    var basePath = AppDomain.CurrentDomain.BaseDirectory;
    var finalPath = Path.Combine(basePath, fileName + ".csv");
    var header = "";
    var info = typeof(T).GetProperties();
    if (!File.Exists(finalPath))
    {
        var file = File.Create(finalPath);
        file.Close();
        foreach (var prop in typeof(T).GetProperties())
        {
            header += prop.Name + "; ";
        }
        header = header.Substring(0, header.Length - 2);
        sb.AppendLine(header);
        TextWriter sw = new StreamWriter(finalPath, true);
        sw.Write(sb.ToString());
        sw.Close();
    }
    foreach (var obj in genericList)
    {
        sb = new StringBuilder();
        var line = "";
        foreach (var prop in info)
        {
            line += prop.GetValue(obj, null) + "; ";
        }
        line = line.Substring(0, line.Length - 2);
        sb.AppendLine(line);
        TextWriter sw = new StreamWriter(finalPath, true);
        sw.Write(sb.ToString());
        sw.Close();
    }
}

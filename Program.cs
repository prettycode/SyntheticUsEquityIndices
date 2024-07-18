﻿using System.Globalization;

Load(Extract());

static void Load(Dictionary<IndexId, SortedDictionary<DateOnly, IndexPeriodPerformance>> multiIndexReturns, string pathPath = @"..\..\..\data\performance")
{
    var indexToTicker = new Dictionary<IndexId, string>
    {
        [IndexId.TotalStockMarket] = "^USTSM",
        [IndexId.LargeCapBlend] = "^USLCB",
        [IndexId.LargeCapValue] = "^USLCV",
        [IndexId.LargeCapGrowth] = "^USLCG",
        [IndexId.MidCapBlend] = "^USMDB",
        [IndexId.MidCapValue] = "^USMDV",
        [IndexId.MidCapGrowth] = "^USMDG",
        [IndexId.SmallCapBlend] = "^USSCB",
        [IndexId.SmallCapValue] = "^USSCV",
        [IndexId.SmallCapGrowth] = "^USSCG"
    };

    foreach (var (index, returns) in multiIndexReturns)
    {
        var tickerHistoryFilename = Path.Combine(pathPath, $"{indexToTicker[index]}.csv");
        var lines = returns.Select(r => $"{r.Key:yyyy-MM-dd},{r.Value.PeriodReturnPercent:G29}");

        File.Delete(tickerHistoryFilename);
        File.WriteAllLines(tickerHistoryFilename, lines);
    }
}

static Dictionary<IndexId, SortedDictionary<DateOnly, IndexPeriodPerformance>> Extract(string csvFilename = @"..\..\..\data\source\Stock-Index-Data-20220923-Monthly.csv")
{
    var columnIndexToCategory = new Dictionary<int, IndexId>
    {
        [1] = IndexId.TotalStockMarket,
        [3] = IndexId.LargeCapBlend,
        [4] = IndexId.LargeCapValue,
        [5] = IndexId.LargeCapGrowth,
        [6] = IndexId.MidCapBlend,
        [7] = IndexId.MidCapValue,
        [8] = IndexId.MidCapGrowth,
        [9] = IndexId.SmallCapBlend,
        [10] = IndexId.SmallCapValue,
        [11] = IndexId.SmallCapGrowth
    };

    const int headerLinesCount = 1;
    const int dateColumnIndex = 0;

    var returns = new Dictionary<IndexId, SortedDictionary<DateOnly, IndexPeriodPerformance>>();

    foreach (var line in File.ReadLines(csvFilename).Skip(headerLinesCount))
    {
        var cells = line.Split(',');
        var date = DateOnly.Parse(cells[dateColumnIndex]);

        foreach (var (currentCell, cellCategory) in columnIndexToCategory)
        {
            if (decimal.TryParse(cells[currentCell], NumberStyles.Any, CultureInfo.InvariantCulture, out var cellValue))
            {
                if (!returns.TryGetValue(cellCategory, out var value))
                {
                    value = returns[cellCategory] = [];
                }

                value[date] = new IndexPeriodPerformance
                {
                    PeriodStartDate = date,
                    IndexId = cellCategory,
                    PeriodReturnPercent = cellValue
                };
            }
        }
    }

    return returns;
}

enum IndexId
{
    TotalStockMarket,
    LargeCapBlend,
    LargeCapGrowth,
    LargeCapValue,
    MidCapBlend,
    MidCapGrowth,
    MidCapValue,
    SmallCapBlend,
    SmallCapGrowth,
    SmallCapValue
}

readonly record struct IndexPeriodPerformance
{
    public required IndexId IndexId { get; init; }
    public required DateOnly PeriodStartDate { get; init; }
    public required decimal PeriodReturnPercent { get; init; }
}

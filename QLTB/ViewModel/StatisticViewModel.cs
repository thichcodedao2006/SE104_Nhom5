using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace QLTB.ViewModel
{
    public class StatisticViewModel : BaseViewModel
    {
        public ISeries[] MaintenanceSeries { get; set; }
        public Axis[] MaintenanceXAxes { get; set; }
        public Axis[] MaintenanceYAxes { get; set; }

        public ISeries[] DeviceStatusSeries { get; set; }

        public ISeries[] CostSeries { get; set; }

        public Axis[] CostXAxes { get; set; }

        public Axis[] CostYAxes { get; set; }

        public StatisticViewModel()
        {
            MaintenanceSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Số công việc",
                    Values = new int[] { 4, 7, 5, 9, 6, 10 }
                }
            };

            MaintenanceXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new string[] { "T1", "T2", "T3", "T4", "T5", "T6" }
                }
            };

            MaintenanceYAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0
                }
            };

            DeviceStatusSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Name = "Hoạt động",
                    Values = new int[] { 18 }
                },
                new PieSeries<int>
                {
                    Name = "Bảo trì",
                    Values = new int[] { 4 }
                },
                new PieSeries<int>
                {
                    Name = "Hỏng",
                    Values = new int[] { 2 }
                }
            };

            CostSeries =
    [
        new LineSeries<double>
        {
            Values = new double[] { 1200, 1800, 1500, 2400, 2100, 2800 },

            Name = "Chi phí",

            Fill = null,

            GeometrySize = 12,

            Stroke = new SolidColorPaint(new SKColor(37, 99, 235), 4),

            GeometryStroke = new SolidColorPaint(new SKColor(37, 99, 235), 4),

            GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255))
        }
    ];

            CostXAxes =
            [
                new Axis
        {
            Labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6" },

            LabelsRotation = 0,

            TextSize = 14,

            SeparatorsPaint = null
        }
            ];

            CostYAxes =
            [
                new Axis
        {
            TextSize = 14,

            Labeler = value => $"${value}",

            MinStep = 500
        }
            ];
        }
    }
}

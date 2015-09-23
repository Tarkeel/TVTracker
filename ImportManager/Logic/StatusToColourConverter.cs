using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Windows.Media;

using ImportManager.Types;

namespace ImportManager.Logic
{
    class StatusToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Enum)
            {
                EpisodeImportRow.ImportStatus status = (EpisodeImportRow.ImportStatus)value;
                switch (status)
                {
                    case EpisodeImportRow.ImportStatus.Conflicting:
                        return new SolidColorBrush(Colors.Red);
                    case EpisodeImportRow.ImportStatus.ManuallySelected:
                        return new SolidColorBrush(Colors.Green);
                    case EpisodeImportRow.ImportStatus.NonConflicting:
                        return new SolidColorBrush(Colors.LightGreen);
                }
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

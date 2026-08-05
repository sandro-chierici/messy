using System.Collections.ObjectModel;

namespace DataService.Business.IO.DataView;

public interface IViewCustomProps
{
    Dictionary<string, string?> CustomProperties { get; set; }
}

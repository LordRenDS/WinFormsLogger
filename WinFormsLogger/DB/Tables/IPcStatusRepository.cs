using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

public interface IPcStatusRepository
{
    IEnumerable<PcStatus> GetAll();
    PcStatus? GetById(int id);
    int Create(PcStatus status);
    int Update(PcStatus status);
    int Delete(int id);
}

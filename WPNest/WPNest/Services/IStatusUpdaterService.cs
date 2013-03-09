using System.Threading.Tasks;

namespace WPNest.Services {

	public interface IStatusUpdaterService {
		Structure CurrentStructure { get; set; }
		void Start();
		void Stop();
		Task UpdateStatusAsync();
	}
}

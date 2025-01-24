using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuoraConnect.Services
{

    public class PagesStepper
    {
        // A variável que você deseja compartilhar entre páginas
        private int _page;

        public int page
        {
            get => _page;
            set
            {
                if (_page != value)
                {
                    _page = value;
                    OnPageChanged?.Invoke();
                }
            }
        }

        // Método para modificar o valor
        public void SetValue(int newValue)
        {
            page = newValue; // Isso agora irá disparar o evento de mudança
        }

        // Evento para notificar a mudança
        public event Action OnPageChanged;
    }

}

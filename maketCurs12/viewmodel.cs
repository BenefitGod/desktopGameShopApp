using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using maketCurs12.Models;
using maketCurs12.Commands;
using System.Linq;
using System.Collections.Generic;

namespace maketCurs12.ViewModels
{
    public class GamesViewModel : INotifyPropertyChanged
    {
        // Оригинальные коллекции (все игры)
        private List<Game> _allMyGames;
        private List<Game> _allNotAddedGames;

        // Отображаемые коллекции (отфильтрованные)
        public ObservableCollection<Game> MyGames { get; set; }
        public ObservableCollection<Game> NotAddedGames { get; set; }

        // Состояния панелей
        private bool _isFirstPanelExpanded = true;
        private bool _isSecondPanelExpanded = true;

        public bool IsFirstPanelExpanded
        {
            get => _isFirstPanelExpanded;
            set
            {
                _isFirstPanelExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FirstPanelVisibility));
                OnPropertyChanged(nameof(FirstPanelIcon));
            }
        }

        public bool IsSecondPanelExpanded
        {
            get => _isSecondPanelExpanded;
            set
            {
                _isSecondPanelExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecondPanelVisibility));
                OnPropertyChanged(nameof(SecondPanelIcon));
            }
        }

        // Видимость панелей
        public System.Windows.Visibility FirstPanelVisibility =>
            IsFirstPanelExpanded ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SecondPanelVisibility =>
            IsSecondPanelExpanded ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        // Иконки для кнопок сворачивания
        public string FirstPanelIcon => IsFirstPanelExpanded ? "▼" : "▶";
        public string SecondPanelIcon => IsSecondPanelExpanded ? "▼" : "▶";

        // Свойство для поиска
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterGames(); // Фильтруем игры при изменении текста
            }
        }

        // Команды
        public ICommand ToggleFirstPanelCommand { get; set; }
        public ICommand ToggleSecondPanelCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }

        public GamesViewModel()
        {
            MyGames = new ObservableCollection<Game>();
            NotAddedGames = new ObservableCollection<Game>();

            ToggleFirstPanelCommand = new RelayCommand(ToggleFirstPanel);
            ToggleSecondPanelCommand = new RelayCommand(ToggleSecondPanel);
            SearchCommand = new RelayCommand(SearchGames);
            ClearSearchCommand = new RelayCommand(ClearSearch);

            LoadGames();
        }

        private void LoadGames()
        {
            _allMyGames = new List<Game>();
            _allNotAddedGames = new List<Game>();

            string path2 = "C:\\Users\\voido\\Desktop\\материал для фото";

            // Мои игры (установленные)

                _allMyGames.Add(new Game
                {
                    Name = $"Cyberpunk 2077",
                    Genre = "RPG / Action",
                    IconPath = "C:\\Users\\voido\\Downloads\\icons\\cyberpunk.png",
                    Status = "Установлена",
                    StatusColor = "#4CAF50",
                    IconColor = "#4CAF50",
                    ImagePath = $"{path2}\\cyber.png"
                });

            _allMyGames.Add(new Game
            {
                Name = $"Deception",
                Genre = "RPG / Action",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\cyberpunk.png",
                Status = "Установлена",
                StatusColor = "#4CAF50",
                IconColor = "#4CAF50",
                ImagePath = $"{path2}\\Supernatural.png"
            });


            _allMyGames.Add(new Game
            {
                Name = $"I Wanna Be The Boshy",
                Genre = "Platformer",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\cyberpunk.png",
                Status = "Не установлена",
                StatusColor = "#4CAF50",
                IconColor = "#4CAF50",
                ImagePath = $"{path2}\\boshy.jpg"
            });

            _allMyGames.Add(new Game
            {
                Name = "The Witcher 3",
                Genre = "RPG / Fantasy",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\witcher.png",
                Status = "Установлена",
                StatusColor = "#4CAF50",
                IconColor = "#2196F3",
                ImagePath = "C:\\Users\\voido\\Desktop\\visual\\1\\сельскийпрайм\\experiment\\maketCurs12\\maketCurs12\\материал для фото\\кингдом.png"
            });

            // Не добавленные игры
            _allNotAddedGames.Add(new Game
            {
                Name = "Dota 2",
                Genre = "MOBA",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\dota.png",
                Status = "Не установлена",
                StatusColor = "#9E9E9E",
                IconColor = "#FF5722",
                ImagePath = "C:\\Users\\voido\\Desktop\\visual\\1\\сельскийпрайм\\experiment\\maketCurs12\\maketCurs12\\материал для фото\\кингдом.png"
            });

            _allNotAddedGames.Add(new Game
            {
                Name = "Counter-Strike 2",
                Genre = "Шутер",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\cs2.png",
                Status = "Не установлена",
                StatusColor = "#9E9E9E",
                IconColor = "#2196F3",
                ImagePath = "C:\\Users\\voido\\Desktop\\visual\\1\\сельскийпрайм\\experiment\\maketCurs12\\maketCurs12\\материал для фото\\кингдом.png"
            });

            _allNotAddedGames.Add(new Game
            {
                Name = "Team Fortress 2",
                Genre = "Шутер",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\tf2.png",
                Status = "Не установлена",
                StatusColor = "#9E9E9E",
                IconColor = "#FF9800",
                ImagePath = "C:\\Users\\voido\\Desktop\\visual\\1\\сельскийпрайм\\experiment\\maketCurs12\\maketCurs12\\материал для фото\\кингдом.png"
            });

            _allNotAddedGames.Add(new Game
            {
                Name = "Rust",
                Genre = "Выживание",
                IconPath = "C:\\Users\\voido\\Downloads\\icons\\rust.png",
                Status = "Не установлена",
                StatusColor = "#9E9E9E",
                IconColor = "#F44336",
                ImagePath = "C:\\Users\\voido\\Desktop\\visual\\1\\сельскийпрайм\\experiment\\maketCurs12\\maketCurs12\\материал для фото\\кингдом.png"
            });

            // Загружаем все игры в отображаемые коллекции
            RefreshGames();
        }

        private void RefreshGames()
        {
            MyGames.Clear();
            foreach (var game in _allMyGames)
                MyGames.Add(game);

            NotAddedGames.Clear();
            foreach (var game in _allNotAddedGames)
                NotAddedGames.Add(game);
        }
        public void AddGame(Game newGame)
        {
            // Добавляем игру в список "Мои игры"
            _allMyGames.Add(newGame);
            MyGames.Add(newGame);

            // Обновляем отображение
            OnPropertyChanged(nameof(MyGames));
        }
        private void FilterGames()
        {
            // Очищаем отображаемые коллекции
            MyGames.Clear();
            NotAddedGames.Clear();

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                // Если поиск пустой - показываем все игры
                foreach (var game in _allMyGames)
                    MyGames.Add(game);
                foreach (var game in _allNotAddedGames)
                    NotAddedGames.Add(game);
            }
            else
            {
                // Фильтруем игры по названию (регистронезависимо)
                var filteredMyGames = _allMyGames
                    .Where(g => g.Name.ToLower().Contains(_searchText.ToLower()));

                var filteredNotAddedGames = _allNotAddedGames
                    .Where(g => g.Name.ToLower().Contains(_searchText.ToLower()));

                foreach (var game in filteredMyGames)
                    MyGames.Add(game);
                foreach (var game in filteredNotAddedGames)
                    NotAddedGames.Add(game);
            }
        }

        private void ToggleFirstPanel(object parameter)
        {
            IsFirstPanelExpanded = !IsFirstPanelExpanded;
        }

        private void ToggleSecondPanel(object parameter)
        {
            IsSecondPanelExpanded = !IsSecondPanelExpanded;
        }

        private void SearchGames(object parameter)
        {
            // Принудительно применяем фильтр по текущему SearchText
            FilterGames();
        }

        private void ClearSearch(object parameter)
        {
            SearchText = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
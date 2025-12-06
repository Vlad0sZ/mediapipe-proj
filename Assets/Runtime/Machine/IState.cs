namespace Runtime.Machine
{
    public interface IState
    {
        void Activate();

        void Deactivate();
    }


    /*
     * Стейт главного меню:
     *  1. UI Controller
     *
     *
     * Стейт игры (начало):
     *  1. UI Controller
     *  2. Avatar Controller
     *
     * Стейт игры (сама игра):
     *  1. UI Controller
     *  2. Timer
     *  3. Lifecycle и тд
     *
     *
     * Стейт окончания игры:
     *  1. UI Controller
     *  2. Счетчик очков
     */
}
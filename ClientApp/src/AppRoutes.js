import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import { DealDeskChat } from "./components/DealDeskChat";

const AppRoutes = [
  {
    index: true,
    element: <DealDeskChat />
  },
  {
    path: '/home',
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  }
];

export default AppRoutes;

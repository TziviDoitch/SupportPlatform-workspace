import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { theme } from 'antd';

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

interface Props {
  labels: string[];
  values: number[];
  seriesLabel: string;
  height?: number;
}

export const BarChart = ({ labels, values, seriesLabel, height = 260 }: Props) => {
  const { token } = theme.useToken();
  return (
    <div style={{ height }}>
      <Bar
        data={{
          labels,
          datasets: [
            {
              label: seriesLabel,
              data: values,
              backgroundColor: token.colorPrimary,
              borderRadius: 4,
            },
          ],
        }}
        options={{
          responsive: true,
          maintainAspectRatio: false,
          locale: 'he-IL',
          plugins: {
            legend: { display: false },
            tooltip: { rtl: true, titleAlign: 'right', bodyAlign: 'right' },
          },
          scales: {
            x: { grid: { display: false } },
            y: { beginAtZero: true, ticks: { precision: 0 } },
          },
        }}
      />
    </div>
  );
};

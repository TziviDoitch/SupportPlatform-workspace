import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { theme } from 'antd';

// Register only the pieces a categorical bar chart needs — keeps the bundle lean and is required
// once per app for the tree-shaken `chart.js` build.
ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

interface Props {
  labels: string[];
  values: number[];
  /** Dataset label, shown in the tooltip. */
  seriesLabel: string;
  height?: number;
}

/** Generic single-series bar chart over `react-chartjs-2`. No domain knowledge. */
export function BarChart({ labels, values, seriesLabel, height = 260 }: Props) {
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
}

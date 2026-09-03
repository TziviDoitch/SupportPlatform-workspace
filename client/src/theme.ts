import type { ThemeConfig } from 'antd';

/** Dark-purple accent for section / card-title / nav icons — the one place this colour is defined. */
export const SECTION_ICON_COLOR = '#5b21b6';

/**
 * The one place the app's look is decided. A calm indigo-on-slate palette, roomy radii and a
 * single quiet elevation — tuned for dense Hebrew data screens. Components read these tokens, so
 * screens never hard-code colours or spacing.
 */
export const theme: ThemeConfig = {
  token: {
    colorPrimary: '#3a5bd9',
    colorInfo: '#3a5bd9',
    colorTextHeading: '#141b2d',
    colorText: '#293142',
    colorBgLayout: '#f3f5fb',
    colorBorderSecondary: '#e7eaf3',
    borderRadius: 10,
    fontFamily: "'Assistant', -apple-system, 'Segoe UI', system-ui, sans-serif",
    fontSize: 14,
    controlHeight: 38,
    boxShadow: '0 1px 2px rgba(20, 27, 45, 0.04), 0 6px 20px rgba(20, 27, 45, 0.06)',
    boxShadowSecondary: '0 6px 24px rgba(20, 27, 45, 0.10)',
    // Borderless cards paint this — a faint dark-purple lift around every card frame.
    boxShadowTertiary: '0 2px 8px rgba(76, 29, 149, 0.06), 0 12px 32px rgba(76, 29, 149, 0.09)',
  },
  components: {
    Layout: { headerBg: '#ffffff', headerHeight: 60, bodyBg: '#f3f5fb' },
    Card: { borderRadiusLG: 16, paddingLG: 22, headerFontSize: 16 },
    Table: {
      headerBg: '#eef1f9',
      headerColor: '#5b6579',
      headerSplitColor: 'transparent',
      borderColor: '#eceef5',
      rowHoverBg: '#f5f7ff',
      cellPaddingBlock: 12,
      headerBorderRadius: 10,
    },
    Button: { fontWeight: 600, primaryShadow: 'none', defaultShadow: 'none' },
    Select: { optionSelectedBg: '#eef2ff', optionSelectedColor: '#141b2d' },
    Menu: { itemSelectedColor: '#3a5bd9', itemSelectedBg: '#eef2ff', itemBorderRadius: 8 },
    Input: { paddingBlock: 7 },
    Alert: { borderRadiusLG: 12 },
    Tag: { borderRadiusSM: 6 },
  },
};

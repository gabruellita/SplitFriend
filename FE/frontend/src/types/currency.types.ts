export interface Currency {
  id:     number;
  code:   string;
  name:   string;
  symbol: string;
}

export interface ConvertResult {
  from:   string;
  to:     string;
  amount: number;
  rate:   number;
  result: number;
  date:   string;
}

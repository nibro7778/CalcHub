import React, { useState } from 'react';
import { Calculator, DollarSign, Users, Clock, AlertCircle, CheckCircle, Info, HelpCircle, Github, Linkedin } from 'lucide-react';

const ChildCareSubsidyCalculator = () => {
  const [formData, setFormData] = useState({
    annualFamilyIncome: '',
    childCareType: '1',
    hourlyRate: '',
    hoursPerWeek: '',
    numberOfChildren: '1',
    activityLevel: '3',
    isWorkingOrStudying: true
  });

  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [activeTooltip, setActiveTooltip] = useState(null);

  const childCareTypes = [
    { id: 1, name: 'Long Day Care' },
    { id: 2, name: 'Family Day Care' },
    { id: 3, name: 'Outside School Hours Care' },
    { id: 4, name: 'In Home Care' },
    { id: 5, name: 'Occasional Care' }
  ];

  const activityLevels = [
    { id: 1, name: '8-16 hours per fortnight', hours: 36 },
    { id: 2, name: '16-48 hours per fortnight', hours: 72 },
    { id: 3, name: '48+ hours per fortnight', hours: 100 }
  ];

  // Field information/help text
  const fieldInfo = {
    annualFamilyIncome: {
      title: "Annual Family Income",
      description: "Your combined family income before tax for the current financial year. Include income from all sources such as wages, salary, business income, and investment returns.",
      example: "Example: If you earn $60,000 and your partner earns $45,000, enter $105,000",
      tips: ["Include both parents' income", "Use your taxable income", "Don't include government payments like Family Tax Benefit"]
    },
    childCareType: {
      title: "Type of Child Care",
      description: "Select the type of approved child care service your child attends. Different care types have different hourly rate caps.",
      example: "Long Day Care is the most common type, typically operating 8+ hours per day",
      tips: ["Must be an approved provider", "Check with your provider if unsure", "Different caps apply to different care types"]
    },
    hourlyRate: {
      title: "Hourly Rate Charged",
      description: "The amount your child care provider charges per hour, before any subsidies are applied. This is the 'gap fee' shown on your statement.",
      example: "Example: If your provider charges $12.50 per hour, enter 12.50",
      tips: ["Check your child care statement", "This is before CCS is applied", "Rate may vary by age of child"]
    },
    hoursPerWeek: {
      title: "Hours of Care Per Week",
      description: "The average number of hours of care your child receives each week. This should be the actual hours used, not the maximum booked.",
      example: "Example: If your child attends 8 hours per day, 5 days a week, enter 40",
      tips: ["Use average weekly hours", "Count only actual care hours", "Can be less than booked hours"]
    },
    numberOfChildren: {
      title: "Number of Children",
      description: "The total number of children in your family who are using child care services covered by CCS.",
      example: "Example: If you have 2 children in child care, enter 2",
      tips: ["Count only children using approved care", "Must be 13 years or younger", "Or up to 18 if they have a disability"]
    },
    activityLevel: {
      title: "Activity Level",
      description: "Your combined family activity level based on work, study, training, or looking for work. This determines the maximum subsidised hours per fortnight.",
      example: "48+ hours includes full-time work (76+ hours per fortnight combined)",
      tips: ["Combine both parents' hours", "Includes work, study, and job search", "Part-time work qualifies for subsidy"]
    },
    isWorkingOrStudying: {
      title: "Activity Test",
      description: "To be eligible for CCS, you must meet the activity test by working, training, studying, or looking for work. Some exemptions apply.",
      example: "You meet this test if you or your partner work at least 8 hours per fortnight",
      tips: ["Includes paid work and study", "Volunteering may count", "Exemptions for special circumstances"]
    }
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    setError('');
  };

  const handleCalculate = async () => {
    setLoading(true);
    setError('');
    setResult(null);

    try {
      // Use runtime config (set in config.js based on hostname)
      const apiUrl = window.ENV?.VITE_API_URL || import.meta.env.VITE_API_URL || 'http://localhost:5144';
      
      console.log('API URL:', apiUrl); // Debug log
      
      const response = await fetch(`${apiUrl}/api/calculators/child-care-subsidy`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          annualFamilyIncome: parseFloat(formData.annualFamilyIncome),
          childCareType: parseInt(formData.childCareType),
          hourlyRate: parseFloat(formData.hourlyRate),
          hoursPerWeek: parseInt(formData.hoursPerWeek),
          numberOfChildren: parseInt(formData.numberOfChildren),
          activityLevel: parseInt(formData.activityLevel),
          isWorkingOrStudying: formData.isWorkingOrStudying
        })
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || 'Calculation failed');
      }

      setResult(data);
    } catch (err) {
      setError(err.message || 'Failed to calculate subsidy. Please check your inputs.');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-AU', {
      style: 'currency',
      currency: 'AUD',
      minimumFractionDigits: 2
    }).format(amount);
  };

  const InfoTooltip = ({ fieldKey }) => {
    const info = fieldInfo[fieldKey];
    const isActive = activeTooltip === fieldKey;

    const handleToggle = (e) => {
      e.preventDefault();
      e.stopPropagation();
      setActiveTooltip(isActive ? null : fieldKey);
    };

    const handleClose = (e) => {
      e.preventDefault();
      e.stopPropagation();
      setActiveTooltip(null);
    };

    return (
      <div className="relative inline-block">
        <button
          type="button"
          onClick={handleToggle}
          className="ml-2 text-blue-600 hover:text-blue-800 focus:outline-none transition-colors"
          aria-label="More information"
        >
          <HelpCircle className="w-5 h-5" />
        </button>
        
        {isActive && (
          <>
            <div 
              className="fixed inset-0 z-10" 
              onClick={handleClose}
            />
            <div className="absolute left-0 top-8 z-20 w-80 bg-white border-2 border-blue-200 rounded-lg shadow-xl p-4">
              <div className="flex items-start justify-between gap-2 mb-2">
                <div className="flex items-start gap-2">
                  <Info className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                  <h4 className="font-bold text-gray-900">{info.title}</h4>
                </div>
                <button
                  type="button"
                  onClick={handleClose}
                  className="text-gray-400 hover:text-gray-600 flex-shrink-0 transition-colors"
                  aria-label="Close"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              
              <p className="text-sm text-gray-700 mb-3">{info.description}</p>
              
              <div className="bg-blue-50 border-l-4 border-blue-400 p-3 mb-3">
                <p className="text-sm text-blue-900 font-medium">{info.example}</p>
              </div>
              
              <div className="space-y-1">
                <p className="text-xs font-semibold text-gray-700 uppercase">Tips:</p>
                {info.tips.map((tip, index) => (
                  <div key={index} className="flex items-start gap-2">
                    <span className="text-blue-600 text-xs mt-1">•</span>
                    <p className="text-xs text-gray-600">{tip}</p>
                  </div>
                ))}
              </div>
            </div>
          </>
        )}
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-8 px-4">
      <div className="max-w-6xl mx-auto">
        <div className="bg-white rounded-2xl shadow-xl overflow-hidden">
          {/* Header */}
          <div className="bg-gradient-to-r from-blue-600 to-indigo-600 px-8 py-6">
            <div className="flex items-center gap-3">
              <Calculator className="w-8 h-8 text-white" />
              <div>
                <h1 className="text-3xl font-bold text-white">
                  Child Care Subsidy Calculator
                </h1>
                <p className="text-blue-100 mt-1">
                  Calculate your Australian Government Child Care Subsidy (2024-25)
                </p>
              </div>
            </div>
          </div>

          <div className="grid md:grid-cols-2 gap-8 p-8">
            {/* Form Section */}
            <div>
              <div className="space-y-6">
                {/* Annual Income */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <DollarSign className="w-4 h-4 mr-2" />
                    <span>Annual Family Income</span>
                    <InfoTooltip fieldKey="annualFamilyIncome" />
                  </div>
                  <input
                    type="number"
                    name="annualFamilyIncome"
                    value={formData.annualFamilyIncome}
                    onChange={handleInputChange}
                    placeholder="e.g., 85000"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <p className="text-xs text-gray-500 mt-1">Combined income before tax</p>
                </div>

                {/* Child Care Type */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <span>Type of Child Care</span>
                    <InfoTooltip fieldKey="childCareType" />
                  </div>
                  <select
                    name="childCareType"
                    value={formData.childCareType}
                    onChange={handleInputChange}
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {childCareTypes.map(type => (
                      <option key={type.id} value={type.id}>{type.name}</option>
                    ))}
                  </select>
                </div>

                {/* Hourly Rate */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <DollarSign className="w-4 h-4 mr-2" />
                    <span>Hourly Rate Charged</span>
                    <InfoTooltip fieldKey="hourlyRate" />
                  </div>
                  <input
                    type="number"
                    name="hourlyRate"
                    value={formData.hourlyRate}
                    onChange={handleInputChange}
                    placeholder="e.g., 12.50"
                    step="0.01"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <p className="text-xs text-gray-500 mt-1">Provider's hourly rate before subsidy</p>
                </div>

                {/* Hours Per Week */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <Clock className="w-4 h-4 mr-2" />
                    <span>Hours of Care Per Week</span>
                    <InfoTooltip fieldKey="hoursPerWeek" />
                  </div>
                  <input
                    type="number"
                    name="hoursPerWeek"
                    value={formData.hoursPerWeek}
                    onChange={handleInputChange}
                    placeholder="e.g., 40"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <p className="text-xs text-gray-500 mt-1">Average weekly hours of care</p>
                </div>

                {/* Number of Children */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <Users className="w-4 h-4 mr-2" />
                    <span>Number of Children</span>
                    <InfoTooltip fieldKey="numberOfChildren" />
                  </div>
                  <input
                    type="number"
                    name="numberOfChildren"
                    value={formData.numberOfChildren}
                    onChange={handleInputChange}
                    min="1"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <p className="text-xs text-gray-500 mt-1">Children using child care</p>
                </div>

                {/* Activity Level */}
                <div>
                  <div className="flex items-center text-sm font-semibold text-gray-700 mb-2">
                    <span>Activity Level (Work/Study)</span>
                    <InfoTooltip fieldKey="activityLevel" />
                  </div>
                  <select
                    name="activityLevel"
                    value={formData.activityLevel}
                    onChange={handleInputChange}
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    {activityLevels.map(level => (
                      <option key={level.id} value={level.id}>
                        {level.name} (up to {level.hours}hrs subsidy)
                      </option>
                    ))}
                  </select>
                  <p className="text-xs text-gray-500 mt-1">Combined family work/study hours</p>
                </div>

                {/* Working/Studying Checkbox */}
                <div className="p-4 bg-blue-50 rounded-lg border border-blue-200">
                  <div className="flex items-start gap-3">
                    <input
                      type="checkbox"
                      name="isWorkingOrStudying"
                      checked={formData.isWorkingOrStudying}
                      onChange={handleInputChange}
                      className="w-5 h-5 text-blue-600 rounded focus:ring-2 focus:ring-blue-500 mt-0.5"
                    />
                    <div className="flex-1">
                      <div className="flex items-center text-sm text-gray-700 font-medium">
                        <span>I meet the activity test requirements</span>
                        <InfoTooltip fieldKey="isWorkingOrStudying" />
                      </div>
                      <p className="text-xs text-gray-600 mt-1">
                        Working, training, studying or looking for work
                      </p>
                    </div>
                  </div>
                </div>

                {/* Error Message */}
                {error && (
                  <div className="flex items-start gap-2 p-4 bg-red-50 border border-red-200 rounded-lg">
                    <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                    <p className="text-sm text-red-800">{error}</p>
                  </div>
                )}

                {/* Submit Button */}
                <button
                  onClick={handleCalculate}
                  disabled={loading}
                  className="w-full bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-4 rounded-lg font-semibold hover:from-blue-700 hover:to-indigo-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
                >
                  {loading ? 'Calculating...' : 'Calculate Subsidy'}
                </button>
              </div>
            </div>

            {/* Results Section */}
            <div>
              {result ? (
                <div className="space-y-4">
                  {/* Eligibility Status */}
                  <div className={`p-4 rounded-lg border-2 ${
                    result.subsidyPercentage > 0 
                      ? 'bg-green-50 border-green-200' 
                      : 'bg-red-50 border-red-200'
                  }`}>
                    <div className="flex items-start gap-3">
                      {result.subsidyPercentage > 0 ? (
                        <CheckCircle className="w-6 h-6 text-green-600 flex-shrink-0" />
                      ) : (
                        <AlertCircle className="w-6 h-6 text-red-600 flex-shrink-0" />
                      )}
                      <div>
                        <h3 className={`font-semibold ${
                          result.subsidyPercentage > 0 ? 'text-green-800' : 'text-red-800'
                        }`}>
                          {result.subsidyPercentage > 0 ? 'Eligible' : 'Not Eligible'}
                        </h3>
                        <p className={`text-sm mt-1 ${
                          result.subsidyPercentage > 0 ? 'text-green-700' : 'text-red-700'
                        }`}>
                          {result.eligibilityMessage}
                        </p>
                      </div>
                    </div>
                  </div>

                  {/* Subsidy Percentage */}
                  {result.subsidyPercentage > 0 && (
                    <>
                      <div className="bg-gradient-to-br from-blue-600 to-indigo-600 p-6 rounded-xl text-white">
                        <div className="text-center">
                          <p className="text-blue-100 text-sm font-medium">Your Subsidy Rate</p>
                          <p className="text-5xl font-bold mt-2">{result.subsidyPercentage}%</p>
                          <p className="text-blue-100 text-sm mt-2">
                            Hourly cap: {formatCurrency(result.hourlyCap)}
                          </p>
                        </div>
                      </div>

                      {/* Cost Breakdown */}
                      <div className="bg-gray-50 rounded-xl p-6 space-y-4">
                        <h3 className="font-bold text-gray-900 text-lg mb-4">Cost Breakdown</h3>
                        
                        <div className="space-y-3">
                          <div className="flex justify-between items-center pb-3 border-b border-gray-200">
                            <span className="text-gray-600">Per Hour</span>
                            <div className="text-right">
                              <p className="font-semibold text-green-600">
                                {formatCurrency(result.subsidyPerHour)}
                              </p>
                              <p className="text-sm text-gray-500">
                                You pay: {formatCurrency(result.outOfPocketPerHour)}
                              </p>
                            </div>
                          </div>

                          <div className="flex justify-between items-center pb-3 border-b border-gray-200">
                            <span className="text-gray-600">Per Week</span>
                            <div className="text-right">
                              <p className="font-semibold text-green-600">
                                {formatCurrency(result.subsidyPerWeek)}
                              </p>
                              <p className="text-sm text-gray-500">
                                You pay: {formatCurrency(result.outOfPocketPerWeek)}
                              </p>
                            </div>
                          </div>

                          <div className="flex justify-between items-center pb-3 border-b border-gray-200">
                            <span className="text-gray-600">Per Fortnight</span>
                            <div className="text-right">
                              <p className="font-semibold text-green-600">
                                {formatCurrency(result.subsidyPerFortnight)}
                              </p>
                              <p className="text-sm text-gray-500">
                                You pay: {formatCurrency(result.outOfPocketPerFortnight)}
                              </p>
                            </div>
                          </div>

                          <div className="flex justify-between items-center pt-2">
                            <span className="font-semibold text-gray-900">Per Year</span>
                            <div className="text-right">
                              <p className="font-bold text-lg text-green-600">
                                {formatCurrency(result.subsidyPerYear)}
                              </p>
                              <p className="text-sm text-gray-500">
                                You pay: {formatCurrency(result.outOfPocketPerYear)}
                              </p>
                            </div>
                          </div>
                        </div>
                      </div>

                      {/* Additional Info */}
                      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                        <p className="text-sm text-blue-800">
                          <strong>Note:</strong> You're eligible for up to {result.subsidisedHoursPerFortnight} {" "}
                           subsidised hours per fortnight based on your activity level.
                        </p>
                      </div>
                    </>
                  )}
                </div>
              ) : (
                <div className="h-full flex items-center justify-center text-center p-8">
                  <div>
                    <Calculator className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                    <p className="text-gray-500">
                      Fill in the details and click "Calculate Subsidy" to see your results
                    </p>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Footer Info */}
          <div className="bg-gray-50 px-8 py-4 border-t">
            <div className="flex flex-col sm:flex-row items-center justify-between gap-3">
              <p className="text-xs text-gray-600 text-center sm:text-left">
                This calculator uses 2024-25 financial year rates. Results are estimates only. 
                Visit Services Australia for official information.
              </p>
              <div className="flex items-center gap-4">
                <span className="text-xs text-gray-500">Built by Niraj Trivedi</span>
                <div className="flex gap-2">
                  <a
                    href="https://github.com/nibro7778"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded-full transition-colors"
                    aria-label="GitHub Profile"
                  >
                    <Github className="w-4 h-4" />
                  </a>
                  <a
                    href="https://www.linkedin.com/in/niraj-trivedi"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="p-2 text-gray-600 hover:text-blue-600 hover:bg-blue-50 rounded-full transition-colors"
                    aria-label="LinkedIn Profile"
                  >
                    <Linkedin className="w-4 h-4" />
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChildCareSubsidyCalculator;
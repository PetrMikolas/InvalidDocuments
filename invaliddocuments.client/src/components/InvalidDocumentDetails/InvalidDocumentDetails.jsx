import './InvalidDocumentDetails.css';
import PropTypes from 'prop-types';

function InvalidDocumentDetails({ details }) {
    return (
        <>
            <p>Doklad s číslem <b>{details.number} byl nalezen</b> v databázi neplatných dokladů.</p>
            <div className="table-container">
                <table className="table table-striped" aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Číslo dokladu</th>
                            <th>Série</th>
                            <th>Typ dokladu</th>
                            <th>Neplatný od</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{details.number}</td>
                            <td>{details.series}</td>
                            <td>{details.type}</td>
                            <td>{details.registeredFrom}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </>
    );
}

InvalidDocumentDetails.propTypes = {
    details: PropTypes.object.isRequired,
};

export default InvalidDocumentDetails;
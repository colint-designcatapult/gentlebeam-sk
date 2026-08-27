#!/usr/bin/env python3
"""Add RELATIONS to test cases in internal file (no explicit UID structure)."""
import json
import re

def load_mappings(mapping_file):
    """Load TC to RQ mappings."""
    with open(mapping_file, 'r') as f:
        return json.load(f)

def process_internal_file(filepath, mappings):
    """
    Process internal file where test cases are implicit.
    Match test cases by TITLE in order.
    """
    with open(filepath, 'r') as f:
        content = f.read()
    
    # Create a mapping: test_case_title -> requirement_uids
    title_to_rq = {item['test_case_title']: item['requirement_uids'] 
                   for item in mappings}
    
    # Find all test case TITLE lines and their positions
    # Pattern: TITLE: ... followed eventually by EXPECTED_BEHAVIOR section
    # Test case TITLEs are within a SECTION with PREFIX: TC-H10SG-
    
    lines = content.split('\n')
    output_lines = []
    i = 0
    in_test_section = False
    last_title = None
    expected_behavior_end = None
    
    while i < len(lines):
        line = lines[i]
        
        # Check if we're in the test case section
        if 'PREFIX: TC-H10SG-' in line:
            in_test_section = True
        
        # If in test section and this is a TITLE line (not the main title)
        if in_test_section and line.startswith('TITLE:') and 'Test Case' in line and 'Test Cases' not in line:
            last_title = line[6:].strip()
        
        # Check if this is an EXPECTED_BEHAVIOR close marker
        if in_test_section and line.strip() == '<<<' and last_title:
            # This closes an EXPECTED_BEHAVIOR section
            expected_behavior_end = i
            output_lines.append(line)
            i += 1
            
            # Check if we should add RELATIONS
            if last_title in title_to_rq and expected_behavior_end is not None:
                requirement_uids = title_to_rq[last_title]
                
                # Check if next non-empty line is already RELATIONS
                next_line_idx = i
                while next_line_idx < len(lines) and lines[next_line_idx].strip() == '':
                    next_line_idx += 1
                
                if next_line_idx < len(lines) and 'RELATIONS:' not in lines[next_line_idx]:
                    # Add RELATIONS section
                    output_lines.append('RELATIONS:')
                    for rq_uid in requirement_uids:
                        output_lines.append('- TYPE: Parent')
                        output_lines.append(f'  VALUE: {rq_uid}')
                        output_lines.append('  ROLE: Verifies')
                    last_title = None
            continue
        
        output_lines.append(line)
        i += 1
    
    return '\n'.join(output_lines)

def main():
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    # Process internal
    internal_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    print("Processing internal...")
    modified = process_internal_file(internal_file, mappings['internal'])
    
    with open(internal_file, 'w') as f:
        f.write(modified)
    
    print(f"✓ Updated internal test protocol")
    
    # Verify count
    count = modified.count('RELATIONS:')
    print(f"  Added {count} RELATIONS sections")

if __name__ == '__main__':
    main()
